// 引入JWT身份验证相关的命名空间，用于处理用户登录和身份验证
using Microsoft.AspNetCore.Authentication.JwtBearer;
// 引入Entity Framework Core的命名空间，用于数据库操作
using Microsoft.EntityFrameworkCore;
// 引入JWT令牌验证相关的命名空间，用于验证JWT令牌的有效性
using Microsoft.IdentityModel.Tokens;
// 引入Swagger/OpenAPI相关的命名空间，用于生成API文档
using Microsoft.OpenApi.Models;
// 引入我们项目中的数据库上下文类
using StudentManagement.Infrastructure.Data;
// 引入文本编码相关的命名空间，用于JWT密钥的编码
using System.Text;

// 创建Web应用程序构建器，这是.NET 6+的新语法
// var关键字表示类型推断，编译器会自动推断builder的类型
var builder = WebApplication.CreateBuilder(args);

// 将控制器服务添加到依赖注入容器中
// 控制器是处理HTTP请求的类，比如处理GET、POST等请求
builder.Services.AddControllers();

// 添加API端点探索器服务，用于发现和生成API文档
// 这是Swagger/OpenAPI功能的基础
builder.Services.AddEndpointsApiExplorer();

// 配置Swagger/OpenAPI文档生成
// c => { } 是一个Lambda表达式，用于配置Swagger选项
builder.Services.AddSwaggerGen(c =>
{
    // 设置API文档的基本信息：标题和版本
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Management API", Version = "v1" });
    
    // 在Swagger中添加JWT身份验证的配置
    // 这样用户就可以在Swagger界面中直接测试需要身份验证的API
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // 描述如何在请求头中使用JWT令牌
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        // 指定认证头的名称
        Name = "Authorization",
        // 指定认证信息在请求中的位置（请求头）
        In = ParameterLocation.Header,
        // 指定认证类型为API密钥
        Type = SecuritySchemeType.ApiKey,
        // 指定认证方案为Bearer（JWT的标准用法）
        Scheme = "Bearer"
    });

    // 为Swagger添加安全要求，表示哪些API需要身份验证
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            // 引用上面定义的Bearer安全方案
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    // 引用类型为安全方案
                    Type = ReferenceType.SecurityScheme,
                    // 引用的安全方案ID
                    Id = "Bearer"
                }
            },
            // 空字符串数组表示没有特定的作用域要求
            new string[] {}
        }
    });
});

// 添加CORS（跨域资源共享）服务
// CORS允许前端（比如React应用）从不同的域名访问这个API
builder.Services.AddCors(options =>
{
    // 创建一个名为"AllowAll"的CORS策略
    options.AddPolicy("AllowAll", policy =>
    {
        // 允许任何来源的请求（开发环境使用，生产环境应该限制）
        policy.AllowAnyOrigin()
              // 允许任何HTTP方法（GET、POST、PUT、DELETE等）
              .AllowAnyMethod()
              // 允许任何请求头
              .AllowAnyHeader();
    });
});

// 将Entity Framework Core的数据库上下文添加到依赖注入容器
// 这样其他类就可以通过构造函数注入来使用数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // 配置数据库连接字符串，从配置文件（appsettings.json）中读取
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加JWT身份验证服务
// JwtBearerDefaults.AuthenticationScheme 是JWT身份验证的默认方案名称
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 配置JWT令牌的验证参数
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 验证令牌的发行者（Issuer）
            ValidateIssuer = true,
            // 验证令牌的受众（Audience）
            ValidateAudience = true,
            // 验证令牌的过期时间
            ValidateLifetime = true,
            // 验证令牌的签名密钥
            ValidateIssuerSigningKey = true,
            // 从配置文件中读取有效的发行者
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // 从配置文件中读取有效的受众
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // 创建用于验证令牌签名的对称密钥
            // ?? 是空合并运算符，如果配置为空则使用默认值
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere"))
        };
    });

// 添加授权服务，用于控制用户访问权限
// 这是身份验证之后的步骤，决定用户能访问哪些资源
builder.Services.AddAuthorization();

// 构建Web应用程序实例
// 这一步会创建所有配置的服务和中间件
var app = builder.Build();

// 配置HTTP请求处理管道
// 中间件的顺序很重要，必须按照正确的顺序配置

// 如果是开发环境，启用Swagger和Swagger UI
if (app.Environment.IsDevelopment())
{
    // 启用Swagger JSON端点
    app.UseSwagger();
    // 启用Swagger UI界面，用于测试API
    app.UseSwaggerUI();
}

// 启用HTTPS重定向中间件
// 如果用户访问HTTP，会自动重定向到HTTPS
app.UseHttpsRedirection();

// 启用CORS中间件，使用之前配置的"AllowAll"策略
// 必须在其他中间件之前启用CORS
app.UseCors("AllowAll");

// 启用身份验证中间件
// 这个中间件会验证JWT令牌并设置用户身份
app.UseAuthentication();

// 启用授权中间件
// 这个中间件会根据用户的角色和权限决定是否允许访问
app.UseAuthorization();

// 将控制器映射到路由
// 这样HTTP请求就能找到对应的控制器方法来处理
app.MapControllers();

// 启动Web应用程序
// 开始监听HTTP请求
app.Run(); 