using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleMatching.Api.Extensions
{
    public static class ExceptionHandlingExtension
    {
        public static void UseMyExceptionHandler(
            this IApplicationBuilder app,
            ILoggerFactory loggerFactory

            )
        {
            // 自己写的错误异常提示
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    //设置状态码
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    // 返回类型
                    context.Response.ContentType = "application/json";

                    //取出异常
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    // 判断异常是否为null
                    if (ex != null)
                    {
                        //创建logger
                        var logger = loggerFactory.CreateLogger("PeopleMatching.Api.Extensions.ExceptionHandlingExtension");
                        //记录日志
                        logger.LogError(500, ex.Error, ex.Error.Message);

                    }
                    // 把response写回去
                    await context.Response.WriteAsync(ex?.Error?.Message ?? "An Error Occurred.");
                });
            });

        }
    }
}
