﻿/*-----------------------------------------------------------------------------
  版 本 号：V1.0 Copyright (c) Coreland.com.  All Rights Reserved.
  创建时间：2017-12-20 16:42:34   创建人：Hujunyuan
  修改时间：                     修改人：          修改内容：
  描    述：
-----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Data;

using CoreLand.Framework;
using CoreLand.Framework.Data;
using CoreLand.Framework.Code;
using EIP.Model;
using EIP.Entity;
using EIP.Model.ViewModels;
namespace EIP.Repository
{
    /// <summary>
    /// 学生信息表仓储
    /// </summary>
    public class GradeRepository : DefaultRepository<Grade>
    {

        public GradeRepository(IUnitOfWork unitOfWork, IRepositoryFactory factory)
            : base(unitOfWork, factory)
        {

        }

        /// <summary>
        /// 查询学生信息表
        /// </summary>
        /// <param name="model">翻页查询基本条件</param>
        /// <param name="totalCount">整体查询结果件数</param>
        /// <returns></returns>
        public List<Grade> QueryGrade(QueryModel model, out int totalCount)
        {
            //查询数据
            var searchKey = (string.IsNullOrEmpty(model.Key) ? "%" : "%" + model.Key.Trim() + "%");
            string sql = "select * from dbo.Grade where LogicDeleteFlag=0 and StudentName like @p0 ";

            //分页查询必须要有排序字段
            model.SortField = string.IsNullOrEmpty(model.SortField) ? "SId" : model.SortField;

            var grades = this.LoadPageEntitiesBySql<Grade>(
                       model.PageIndex,
                       model.PageSize,
                       out totalCount,
                       sql,
                       model.SortField + " " + model.SortOrder,
                       searchKey
                       ).ToList();
            
            return grades;
        }

        /// <summary>
        /// 查询男生女生人数，班级总人数
        /// </summary>
        /// <param name="model">翻页查询基本条件</param>
        /// <param name="totalCount">整体查询结果件数</param>
        /// <returns></returns>
        public List<CountManAndWoman> QueryCountManAndWoman(QueryModel model, out int totalCount)
        {
            //创建查询用sql语句
            string sql = "select Remo.RId,Remo.Remo_id,SUM(case Sex when 'M' then 1 else 0 end) " +
                "as manCount,SUM(case Sex when 'W' then 1 else 0 end) " +
                "as womanCount, SUM(1) as totalCount from Remo inner " +
                "join Grade on Remo.RId = Grade.RId group by Remo.RId,Remo.Remo_id";

            //分页查询排序字段
            model.SortField = string.IsNullOrEmpty(model.SortField) ? "Remo.RId" : model.SortField;

            var grades = this.LoadPageEntitiesBySql<CountManAndWoman>(
                       model.PageIndex,
                       model.PageSize,
                       out totalCount,
                       sql,
                       model.SortField + " " + model.SortOrder
                      // searchKey
                       ).ToList();

            return grades;
        }

        /// <summary>
        /// 根据班级模糊查询男生女生人数，班级总人数
        /// </summary>
        /// <param name="model">翻页查询基本条件</param>
        /// <param name="totalCount">整体查询结果件数</param>
        /// <returns></returns>
        public List<CountManAndWoman> QueryCountManAndWomanUseLike(QueryModel model, out int totalCount)
        {
            var searchKey = (string.IsNullOrEmpty(model.Key) ? "%" : "%" + model.Key.Trim() + "%");
            //创建查询用sql语句
            string sql = "select * from (select Remo.RId,Remo.Remo_id,SUM(case Sex when 'M' then 1 else 0 end) " +
                "as manCount,SUM(case Sex when 'W' then 1 else 0 end) " +
                "as womanCount, SUM(1) as totalCount from Remo inner " +
                "join Grade on Remo.RId = Grade.RId group by Remo.RId,Remo.Remo_id) as T where cast(T.Remo_id as varchar(20)) like @p0";

            //分页查询排序字段
            model.SortField = string.IsNullOrEmpty(model.SortField) ? "T.Remo_id" : model.SortField;

            var grades = this.LoadPageEntitiesBySql<CountManAndWoman>(
                       model.PageIndex,
                       model.PageSize,
                       out totalCount,
                       sql,
                       model.SortField + " " + model.SortOrder,
                       searchKey
                       ).ToList();

            return grades;
        }

        /// <summary>
        /// 用存储过程查询男生女生人数，班级总人数
        /// </summary>
        /// <param name="model">翻页查询基本条件</param>
        /// <param name="totalCount">整体查询结果件数</param>
        /// <returns></returns>
        public List<CountManAndWoman> QueryCountManAndWomanByProc(QueryModel model, out int totalCount)
        {
            totalCount = 0;
            //分页查询排序字段
            var list = this.ExceuteProcedure<CountManAndWoman>("[dbo].[COUNT_MEN_AND_WOMAN]", model.PageIndex,model.PageSize).ToList();
            totalCount = this.ExceuteProcedure<CountManAndWoman>("[dbo].[GET_TOTALCOUNT]").ToList().Count();
            return list;
        }
    }
}
