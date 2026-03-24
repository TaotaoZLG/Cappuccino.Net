using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace Cappuccino.Entity
{
    [Table("SysCaseInfo")]
    public class SysCaseInfoEntity : BaseEntity
    {
        /// <summary>
        /// 业务批次Id
        /// </summary>
        [DisplayName("业务批次Id")]
        [ExcelColumnName("业务批次Id")]
        public string BusinessBatchId { get; set; }

        /// <summary>
        /// 案件编号（格式：机构代码+日期+序列号）
        /// </summary>
        [DisplayName("案件编号")]
        [ExcelColumnName("案件编号")]
        public string CaseNo { get; set; }

        /// <summary>
        /// 案号
        /// </summary>
        [DisplayName("案号")]
        [ExcelColumnName("案号")]
        public string CaseNumber { get; set; }

        /// <summary>
        /// 客户姓名
        /// </summary>
        [DisplayName("客户姓名")]
        [ExcelColumnName("客户姓名")]
        public string CustName { get; set; }

        /// <summary>
        /// 客户性别
        /// </summary>
        [DisplayName("客户性别")]
        [ExcelColumnName("客户性别")]
        public string CustGender { get; set; }

        /// <summary>
        /// 客户民族
        /// </summary>
        [DisplayName("客户民族")]
        [ExcelColumnName("客户民族")]
        public string CustNation { get; set; }

        /// <summary>
        /// 客户出生日期
        /// </summary>
        [DisplayName("出生日期")]
        [ExcelColumnName("出生日期")]
        public DateTime? CustBirthdate { get; set; }

        /// <summary>
        /// 客户身份证号
        /// </summary>
        [DisplayName("客户身份证号")]
        [ExcelColumnName("客户身份证号")]
        public string CustIDNumber { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [DisplayName("银行卡号")]
        [ExcelColumnName("银行卡号")]
        public string CustCardNo { get; set; }

        /// <summary>
        /// 客户邮箱
        /// </summary>
        [DisplayName("客户邮箱")]
        [ExcelColumnName("客户邮箱")]
        public string CustEmail { get; set; }

        /// <summary>
        /// 户籍地址
        /// </summary>
        [DisplayName("户籍地址")]
        [ExcelColumnName("户籍地址")]
        public string HouseholdAddress { get; set; }

        /// <summary>
        /// 住宅地址
        /// </summary>
        [DisplayName("住宅地址")]
        [ExcelColumnName("住宅地址")]
        public string HomeAddress { get; set; }

        /// <summary>
        /// 邮寄地址
        /// </summary>
        [DisplayName("邮寄地址")]
        [ExcelColumnName("邮寄地址")]
        public string MailingAddress { get; set; }

        /// <summary>
        /// 单位地址
        /// </summary>
        [DisplayName("单位地址")]
        [ExcelColumnName("单位地址")]
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [DisplayName("联系电话")]
        [ExcelColumnName("联系电话")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 开户日期
        /// </summary>
        [DisplayName("开户日期")]
        [ExcelColumnName("开户日期")]
        public DateTime? AccountOpenDate { get; set; }

        /// <summary>
        /// 开卡日期
        /// </summary>
        [DisplayName("开卡日期")]
        [ExcelColumnName("开卡日期")]
        public DateTime? CardOpenDate { get; set; }

        /// <summary>
        /// 账户激活日期
        /// </summary>
        [DisplayName("账户激活日期")]
        [ExcelColumnName("账户激活日期")]
        public DateTime? AccountActivateDate { get; set; }

        /// <summary>
        /// 账单日（1-31）
        /// </summary>
        [DisplayName("账单日")]
        [ExcelColumnName("账单日")]
        public byte? BillDay { get; set; }

        /// <summary>
        /// 欠款本金
        /// </summary>
        [DisplayName("欠款本金")]
        [ExcelColumnName("欠款本金")]
        public decimal? PrincipalAmount { get; set; }

        /// <summary>
        /// 利息
        /// </summary>
        [DisplayName("利息")]
        [ExcelColumnName("利息")]
        public decimal? InterestAmount { get; set; }

        /// <summary>
        /// 违约金
        /// </summary>
        [DisplayName("违约金")]
        [ExcelColumnName("违约金")]
        public decimal? PenaltyInterest { get; set; }

        /// <summary>
        /// 分期手续费
        /// </summary>
        [DisplayName("分期手续费")]
        [ExcelColumnName("分期手续费")]
        public string InstallmentFee { get; set; }

        /// <summary>
        /// 人民币费用
        /// </summary>
        [DisplayName("人民币费用")]
        [ExcelColumnName("人民币费用")]
        public decimal? RMBFeeAmount { get; set; }

        /// <summary>
        /// 共计人民币
        /// </summary>
        [DisplayName("共计人民币")]
        [ExcelColumnName("共计人民币")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        [DisplayName("服务费")]
        [ExcelColumnName("服务费")]
        public string ServiceFee { get; set; }

        /// <summary>
        /// 逾期金额
        /// </summary>
        [DisplayName("逾期金额")]
        [ExcelColumnName("逾期金额")]
        public decimal? OverdueAmount { get; set; }

        /// <summary>
        /// 逾期天数
        /// </summary>
        [DisplayName("逾期天数")]
        [ExcelColumnName("逾期天数")]
        public byte? OverdueDays { get; set; }

        /// <summary>
        /// 开始逾期时间
        /// </summary>
        [DisplayName("开始逾期日期")]
        [ExcelColumnName("开始逾期日期")]
        public DateTime? OverdueStartDate { get; set; }

        /// <summary>
        /// 最后一次还款日期
        /// </summary>
        [DisplayName("最后一次还款日期")]
        [ExcelColumnName("最后一次还款日期")]
        public DateTime? LastRepayDate { get; set; }

        /// <summary>
        /// 账单余额
        /// </summary>
        [DisplayName("账单余额")]
        [ExcelColumnName("账单余额")]
        public decimal? BillBalance { get; set; }

        /// <summary>
        /// 本金核算
        /// </summary>
        [DisplayName("本金核算")]
        [ExcelColumnName("本金核算")]
        [StringLength(300)]
        public string PrincipalCalculated { get; set; }

        /// <summary>
        /// 违约金合计
        /// </summary>
        [DisplayName("违约金合计")]
        [ExcelColumnName("违约金合计")]
        public string TotalPenalty { get; set; }

        /// <summary>
        /// 分期手续费合计
        /// </summary>
        [DisplayName("分期手续费合计")]
        [ExcelColumnName("分期手续费合计")]
        public decimal? TotalInstallmentFee { get; set; }

        /// <summary>
        /// 利息截止
        /// </summary>
        [DisplayName("利息截止")]
        [ExcelColumnName("利息截止")]
        public DateTime? InterestEndDate { get; set; }

        /// <summary>
        /// 截止至日
        /// </summary>
        [DisplayName("截止至日")]
        [ExcelColumnName("截止至日")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 诉讼费
        /// </summary>
        [DisplayName("诉讼费")]
        [ExcelColumnName("诉讼费")]
        public decimal? LitigationFee { get; set; }

        /// <summary>
        /// 案件详情（案由）
        /// </summary>
        [DisplayName("案件详情")]
        [ExcelColumnName("案件详情")]
        public string CaseDetails { get; set; }

        /// <summary>
        /// 产品Id（关联SysProduct表Id）
        /// </summary>
        [DisplayName("产品Id")]
        [ExcelColumnName("产品Id")]
        public long? ProductId { get; set; }

        /// <summary>
        /// 案件所在部门Id（关联SysDepartment表Id）
        /// </summary>
        [DisplayName("部门Id")]
        [ExcelColumnName("部门Id")]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// 所在部门变更时间
        /// </summary>
        [DisplayName("部门变更时间")]
        [ExcelColumnName("部门变更时间")]
        public DateTime? DepartmentChangeTime { get; set; }

        /// <summary>
        /// 调解员Id（关联SysUser表Id）
        /// </summary>
        [DisplayName("调解员Id")]
        [ExcelColumnName("调解员Id")]
        public long? MediatorId { get; set; }

        /// <summary>
        /// 调解员变更时间
        /// </summary>
        [DisplayName("调解员变更时间")]
        [ExcelColumnName("调解员变更时间")]
        public DateTime? MediatorChangeTime { get; set; }

        /// <summary>
        /// 归档文件保存路径（虚拟路径）
        /// </summary>
        [DisplayName("归档路径")]
        [ExcelColumnName("归档路径")]
        public string ArchiveVirtualPath { get; set; }

        /// <summary>
        /// 案件备注信息1
        /// </summary>
        [DisplayName("备注信息1")]
        [ExcelColumnName("备注信息1")]
        public string Remark1 { get; set; }

        /// <summary>
        /// 案件备注信息2
        /// </summary>
        [DisplayName("备注信息2")]
        [ExcelColumnName("备注信息2")]
        public string Remark2 { get; set; }

        /// <summary>
        /// 案件备注信息3
        /// </summary>
        [DisplayName("备注信息3")]
        [ExcelColumnName("备注信息3")]
        public string Remark3 { get; set; }

    }
}
