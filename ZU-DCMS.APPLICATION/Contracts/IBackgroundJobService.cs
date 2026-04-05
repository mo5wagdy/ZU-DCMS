using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    public interface IBackgroundJobService
    {
        // Fire and Forget
        // بيشتغل في الـ Background من غير ما ينتظر
        void Enqueue<T>(System.Linq.Expressions.Expression<Action<T>> job);

        // Scheduled Job
        // بيشتغل بعد وقت معين
        void Schedule<T>(
            System.Linq.Expressions.Expression<Action<T>> job,
            TimeSpan delay);

        // Recurring Job
        // بيشتغل على فترات منتظمة
        void AddOrUpdateRecurring<T>(
            string jobId,
            System.Linq.Expressions.Expression<Action<T>> job,
            string cronExpression);
    }
}
