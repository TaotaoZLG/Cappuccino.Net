using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// 雪花算法ID生成器帮助类 (单例模式)
    /// 调用方式：IdGeneratorHelper.Instance.NextId()
    /// </summary>
    public class IdGeneratorHelper
    {
        #region 单例实现

        private static readonly Lazy<IdGeneratorHelper> _instance = new Lazy<IdGeneratorHelper>(() => new IdGeneratorHelper());

        /// <summary>
        /// 全局唯一实例
        /// </summary>
        public static IdGeneratorHelper Instance => _instance.Value;

        #endregion

        #region 配置项 

        // 基准时间 (2023-01-01)，可根据项目实际启动时间调整
        private const long Epoch = 1672531200000L;
        private long _workerId;
        private long _datacenterId;

        #endregion

        #region 内部变量

        private const int TimestampBits = 41;
        private const int DatacenterIdBits = 5;
        private const int WorkerIdBits = 5;
        private const int SequenceBits = 12;

        private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long MaxSequence = -1L ^ (-1L << SequenceBits);

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        private readonly object _lock = new object();
        private bool _initialized = false;

        #endregion

        /// <summary>
        /// 私有构造函数，防止外部 new
        /// 直接在构造函数中完成 WorkerId/DatacenterId 的初始化
        /// </summary>
        private IdGeneratorHelper()
        {
            // 1. 从配置文件读取 WorkerId/DatacenterId，读取失败则用默认值1
            long workerId = long.TryParse(ConfigurationManager.AppSettings["Snowflake.WorkerId"], out var w) ? w : 1;
            long datacenterId = long.TryParse(ConfigurationManager.AppSettings["Snowflake.DatacenterId"], out var d) ? d : 1;

            // 2. 校验参数合法性（复用原 SetConfig 的校验逻辑）
            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"WorkerId 必须在 0 到 {MaxWorkerId} 之间");
            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"DatacenterId 必须在 0 到 {MaxDatacenterId} 之间");

            // 3. 完成初始化
            _workerId = workerId;
            _datacenterId = datacenterId;
            _initialized = true;

            // 可选：添加日志，提示当前使用的 WorkerId/DatacenterId
            // LogHelper.Info($"雪花算法初始化完成，WorkerId={workerId}, DatacenterId={datacenterId}");
        }

        /// <summary>
        /// 如需外部重配置可保留，但构造函数已初始化后调用会抛异常
        /// </summary>
        /// <param name="workerId">工作机器ID (0-31)</param>
        /// <param name="datacenterId">数据中心ID (0-31)</param>
        public void SetConfig(long workerId, long datacenterId)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("ID生成器已初始化并正在使用，无法重新配置！");
            }

            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"WorkerId 必须在 0 到 {MaxWorkerId} 之间");
            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"DatacenterId 必须在 0 到 {MaxDatacenterId} 之间");

            _workerId = workerId;
            _datacenterId = datacenterId;
            _initialized = true;
        }

        /// <summary>
        /// 生成下一个唯一ID
        /// </summary>
        public long NextId()
        {
            lock (_lock)
            {
                long timestamp = GetTimeNow();

                // 时钟回退检查
                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException($"时钟回退！当前时间戳: {timestamp}, 上次时间戳: {_lastTimestamp}");
                }

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0)
                    {
                        // 序列号用完，等待下一毫秒
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Epoch) << TimestampLeftShift) |
                       (_datacenterId << DatacenterIdShift) |
                       (_workerId << WorkerIdShift) |
                       _sequence;
            }
        }

        private long GetTimeNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private long WaitNextMillis(long lastTimestamp)
        {
            long timestamp = GetTimeNow();
            while (timestamp <= lastTimestamp)
            {
                Thread.Sleep(1);
                timestamp = GetTimeNow();
            }
            return timestamp;
        }

        /// <summary>
        /// 从ID解析时间，用于调试
        /// </summary>
        public DateTime GetDateTimeFromId(long id)
        {
            long timestamp = (id >> TimestampLeftShift) + Epoch;
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
        }
    }
}