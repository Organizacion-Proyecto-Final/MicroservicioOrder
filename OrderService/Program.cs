using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands.AddOrderItem;
using OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;
using OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Application.UseCases.Orders.Commands.NotifyOrderReadyByKitchen;
using OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;
using OrderService.Application.UseCases.Orders.Queries.GetAllOrders;
using OrderService.Application.UseCases.Orders.Queries.GetActiveOrdersSummaryByTable;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;
using OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetReadyDeliveryOrders;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;
using OrderService.Application.UseCases.Orders.Queries.GetOrderStatuses;
using OrderService.Application.UseCases.Tables.Commands.CreateTable;
using OrderService.Application.UseCases.Tables.Commands.DeleteTable;
using OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;
using OrderService.Application.UseCases.Tables.Commands.UpdateTable;
using OrderService.Application.UseCases.Tables.Queries.GetAllTables;
using OrderService.Application.UseCases.Tables.Queries.GetTableById;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Persistence;
using OrderService.Presentation.Http;
using OrderService.Presentation.Hubs;
using OrderService.Presentation.Middlewares;
using OrderService.Presentation.Realtime;
using OrderService.Application.Realtime;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.UseCases.Facturation.Queries;
using Application.UseCases.Facturation.Commands;
using Infrastructure.Persistence.Repositories;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IFacturationRepository, FacturationRepository>();


builder.Services.AddScoped<IGetFacturasHandler, GetFacturasHandler>();
builder.Services.AddScoped<IGetFacturationMetricsHandler, GetFacturationMetricsHandler>();
builder.Services.AddScoped<IConfirmPaymentHandler, ConfirmPaymentHandler>();
builder.Services.AddScoped<IConfirmTablePaymentHandler, ConfirmTablePaymentHandler>();



builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthorizationHeaderPropagationHandler>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderNotifier, SignalROrderNotifier>();

const string CorsPolicy = "DefaultCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.SetIsOriginAllowed(origin => origin.Contains("localhost", StringComparison.OrdinalIgnoreCase));
        else
            policy.WithOrigins(allowedOrigins);

        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:Users:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta la configuracion ExternalServices:Users:BaseUrl");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>()
.AddPolicyHandler(HttpResiliencePolicies.Retry)
.AddPolicyHandler(HttpResiliencePolicies.CircuitBreaker);

builder.Services.AddHttpClient<IMenuCatalogClient, MenuCatalogClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:MenuCatalog:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>()
.AddPolicyHandler(HttpResiliencePolicies.Retry)
.AddPolicyHandler(HttpResiliencePolicies.CircuitBreaker);

builder.Services.AddHttpClient<IStockClient, StockClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:Stock:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>()
.AddPolicyHandler(HttpResiliencePolicies.Retry)
.AddPolicyHandler(HttpResiliencePolicies.CircuitBreaker);

builder.Services.AddHttpClient<IKitchenClient, KitchenClient>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ExternalServices:Kitchen:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(2);
})
.AddHttpMessageHandler<AuthorizationHeaderPropagationHandler>()
.AddPolicyHandler(HttpResiliencePolicies.CircuitBreaker);

builder.Services.AddScoped<ICreateOrderCommandHandler, CreateOrderCommandHandler>();
builder.Services.AddScoped<IChangeOrderStatusCommandHandler, ChangeOrderStatusCommandHandler>();
builder.Services.AddScoped<INotifyOrderReadyByKitchenCommandHandler, NotifyOrderReadyByKitchenCommandHandler>();
builder.Services.AddScoped<IAddOrderItemCommandHandler, AddOrderItemCommandHandler>();
builder.Services.AddScoped<IAddNoteToOrderCommandHandler, AddNoteToOrderCommandHandler>();
builder.Services.AddScoped<IUpdateItemStatusCommandHandler, UpdateItemStatusCommandHandler>();
builder.Services.AddScoped<IGetAllOrdersQueryHandler, GetAllOrdersQueryHandler>();
builder.Services.AddScoped<IGetOrdersByStatusQueryHandler, GetOrdersByStatusQueryHandler>();
builder.Services.AddScoped<IGetOrdersByTableQueryHandler, GetOrdersByTableQueryHandler>();
builder.Services.AddScoped<IGetActiveOrdersSummaryByTableQueryHandler, GetActiveOrdersSummaryByTableQueryHandler>();
builder.Services.AddScoped<IGetReadyDeliveryOrdersQueryHandler, GetReadyDeliveryOrdersQueryHandler>();
builder.Services.AddScoped<IGetOrderStatusesQueryHandler, GetOrderStatusesQueryHandler>();
builder.Services.AddScoped<IGetOrderItemStatusesQueryHandler, GetOrderItemStatusesQueryHandler>();
builder.Services.AddScoped<IGetAllTablesQueryHandler, GetAllTablesQueryHandler>();
builder.Services.AddScoped<IGetOrderByIdQueryHandler, GetOrderByIdQueryHandler>();
builder.Services.AddScoped<IGetTableByIdQueryHandler, GetTableByIdQueryHandler>();
builder.Services.AddScoped<ICreateTableCommandHandler, CreateTableCommandHandler>();
builder.Services.AddScoped<IDeleteTableCommandHandler, DeleteTableCommandHandler>();
builder.Services.AddScoped<IUpdateTableCommandHandler, UpdateTableCommandHandler>();
builder.Services.AddScoped<IToggleTableStatusCommandHandler, ToggleTableStatusCommandHandler>();
builder.Services.AddScoped<ICreateInvoiceFromOrdersCommandHandler, CreateInvoiceFromOrdersCommandHandler>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(modelState => modelState.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "La solicitud es invalida." : error.ErrorMessage)
                .ToArray();

            var response = new ErrorResponseDto
            {
                Message = errors.Length == 0 ? "La solicitud es invalida." : string.Join(" ", errors),
                StatusCode = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.UtcNow
            };

            return new BadRequestObjectResult(response);
        };
    });

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Falta la configuracion Jwt:Key.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };

        // SignalR no puede enviar el header Authorization en el handshake WebSocket/SSE,
        // por lo que el token se acepta tambien como query string (?access_token=...)
        // unicamente para las solicitudes dirigidas al hub de Orders.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(OrderHubRoutes.Path))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AppDbInitializer.InitializeAsync(db);
}

if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
    app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>(OrderHubRoutes.Path);
app.MapHealthChecks("/health");
app.Run();
