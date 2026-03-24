using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Model
{
    public class SysCaseInfoModel
    {
        /// <summary>
        /// 业务批次Id
        /// </summary>
        public string BusinessBatchId { get; set; }

        /// <summary>
        /// 案件编号（格式：机构代码+日期+序列号）
        /// </summary>
        public string CaseNo { get; set; }

        /// <summary>
        /// 案号
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 客户性别
        /// </summary>
        public string CustGender { get; set; }

        /// <summary>
        /// 客户民族
        /// </summary>
        public string CustNation { get; set; }

        /// <summary>
        /// 客户出生日期
        /// </summary>
        public string CustBirthdate { get; set; }

        /// <summary>
        /// 客户身份证号
        /// </summary>
        public string CustIDNumber { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CustCardNo { get; set; }

        /// <summary>
        /// 客户邮箱
        /// </summary>
        public string CustEmail { get; set; }

        /// <summary>
        /// 户籍地址
        /// </summary>
        public string HouseholdAddress { get; set; }

        /// <summary>
        /// 住宅地址
        /// </summary>
        public string HomeAddress { get; set; }

        /// <summary>
        /// 邮寄地址
        /// </summary>
        public string MailingAddress { get; set; }

        /// <summary>
        /// 单位地址
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// 开户日期
        /// </summary>
        public string AccountOpenDate { get; set; }

        /// <summary>
        /// 开卡日期
        /// </summary>
        public string CardOpenDate { get; set; }

        /// <summary>
        /// 账户激活日期
        /// </summary>
        public string AccountActivateDate { get; set; }

        /// <summary>
        /// 账单日（1-31）
        /// </summary>
        public string BillDay { get; set; }

        /// <summary>
        /// 欠款本金
        /// </summary>
        public string PrincipalAmount { get; set; }

        /// <summary>
        /// 利息
        /// </summary>
        public string InterestAmount { get; set; }

        /// <summary>
        /// 违约金
        /// </summary>
        public string PenaltyInterest { get; set; }

        /// <summary>
        /// 分期手续费
        /// </summary>
        public string InstallmentFee { get; set; }

        /// <summary>
        /// 人民币费用
        /// </summary>
        public string RMBFeeAmount { get; set; }

        /// <summary>
        /// 共计人民币
        /// </summary>
        public string TotalAmount { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        public string ServiceFee { get; set; }

        /// <summary>
        /// 逾期金额
        /// </summary>
        public string OverdueAmount { get; set; }

        /// <summary>
        /// 逾期天数
        /// </summary>
        public string OverdueDays { get; set; }

        /// <summary>
        /// 开始逾期时间
        /// </summary>
        public string OverdueStartDate { get; set; }

        /// <summary>
        /// 最后一次还款日期
        /// </summary>
        public string LastRepayDate { get; set; }

        /// <summary>
        /// 账单余额
        /// </summary>
        public string BillBalance { get; set; }

        /// <summary>
        /// 本金核算
        /// </summary>
        public string PrincipalCalculated { get; set; }

        /// <summary>
        /// 违约金合计
        /// </summary>
        public string TotalPenalty { get; set; }

        /// <summary>
        /// 分期手续费合计
        /// </summary>
        public string TotalInstallmentFee { get; set; }

        /// <summary>
        /// 利息截止
        /// </summary>
        public string InterestEndDate { get; set; }

        /// <summary>
        /// 截止至日
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 诉讼费
        /// </summary>
        public string LitigationFee { get; set; }

        /// <summary>
        /// 案件详情（案由）
        /// </summary>
        public string CaseDetails { get; set; }

        /// <summary>
        /// 产品Id（关联SysProduct表Id）
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 案件所在部门Id（关联SysDepartment表Id）
        /// </summary>
        public string DepartmentId { get; set; }

        /// <summary>
        /// 所在部门变更时间
        /// </summary>
        public string DepartmentChangeTime { get; set; }

        /// <summary>
        /// 调解员Id（关联SysUser表Id）
        /// </summary>
        public string MediatorId { get; set; }

        /// <summary>
        /// 调解员变更时间
        /// </summary>
        public string MediatorChangeTime { get; set; }

        /// <summary>
        /// 归档文件保存路径（虚拟路径）
        /// </summary>
        public string ArchiveVirtualPath { get; set; }

        /// <summary>
        /// 案件备注信息1
        /// </summary>
        public string Remark1 { get; set; }

        /// <summary>
        /// 案件备注信息2
        /// </summary>
        public string Remark2 { get; set; }

        /// <summary>
        /// 案件备注信息3
        /// </summary>
        public string Remark3 { get; set; }
    }
}