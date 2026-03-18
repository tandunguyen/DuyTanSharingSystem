
using Application.Interface.ChatAI;
using Infrastructure.ChatAI;

namespace Infrastructure
{
    public static class InfastructureService
    {
        public static IServiceCollection AddInfastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext với SQL Server
            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

            Console.WriteLine($"DEBUG: Attempting to use Connection String: '{defaultConnectionString ?? "NULL or NOT FOUND"}'");
            if (string.IsNullOrWhiteSpace(defaultConnectionString))
            {
                // Ném ngoại lệ nếu chuỗi kết nối trống rỗng
                throw new Exception("⚠️ DefaultConnection string không được để trống! Kiểm tra App Settings hoặc appsettings.json.");
            }
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(defaultConnectionString);
            });
            // Đăng ký Redis ConnectionMultiplexer
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "")
            );
            // Đăng ký User Secrets
            services.Configure<GeminiModel>(configuration.GetSection("GoogleGeminiApi"));
            services.Configure<GeminiModel2>(configuration.GetSection("GoogleGeminiApi2"));
            services.Configure<MapsKeyModel>(configuration.GetSection("GoogleMaps"));

           
            var geminiModel = configuration.GetSection("GoogleGeminiApi").Get<GeminiModel>();

            if (geminiModel == null || string.IsNullOrWhiteSpace(geminiModel.ApiKey))
            {
                throw new Exception("⚠️ Jwt:Key không được để trống! Kiểm tra user-secrets hoặc appsettings.json.");
            }



            // Đăng ký Cache Service
            services.AddScoped<ICacheService, RedisCacheService>();


            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IRefreshtokenRepository, RefreshtokenRepository>();
            services.AddScoped<IRidePostRepository, RidePostRepository>();
            services.AddScoped<IRideRepository, RideRepository>();
            services.AddScoped<ILocationUpdateRepository, LocationUpdateRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IRideReportRepository, RideReportRepository>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IAIConversationRepository, AIConversationRepository>();
            services.AddScoped<IAIChatHistoryRepository, AIChatHistoryRepository>();
            services.AddScoped<IUserReportRepository, UserReportRepository>();

            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IShareRepository, ShareRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserScoreHistoriesRepository, UserScoreHistoriesRepository>();
            services.AddScoped<IAccommodationPostRepository, AccommodationPostRepository>();
            services.AddScoped<IAccommodationReviewRepository, AccommodationReviewRepository>();
            services.AddScoped<IStudyMaterialRepository, StudyMaterialRepository>();
            services.AddScoped<IStudyMaterialRatingRepository, StudyMaterialRatingRepository>();

            //đăng kí cho search AI
            services.AddScoped<IDataAIService, DataAIService>();
            services.AddScoped<IApiPythonService, ApiPythonService>();
            //services.AddScoped<ISearchAIService, ApiPythonService2>();
            //đăng kí chat
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageStatusService, MessageStatusService>();
            // ✅ Đăng ký HttpClient
            services.AddHttpClient();

            // ✅ Đăng ký GeminiService
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IGeminiService2, GeminiService2>();
            services.AddScoped<IGeminiAccommodationServices, GeminiAccommodationServices>();
            // Đăng ký Google Maps và HERE Maps nhưng chưa chọn
            services.AddScoped<GoogleMapsService>();
            services.AddScoped<HereMapService>();
            services.AddScoped<TomTomMapService>();
            // Đăng ký Factory
            services.AddScoped<MapServiceFactory>();
            // Đăng ký IMapService thông qua Factory
            services.AddScoped<IMapService>(sp => sp.GetRequiredService<MapServiceFactory>().Create());


            services.AddScoped<IUnitOfWork, UnitOfWork>(); // Đăng ký trước UserService
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();

            //đăng kí hub
            services.AddScoped<ISignalRNotificationService, SignalRNotificationService>(); // Dùng SignalR để gửi thông báo
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LikeEventHandler).Assembly));
            //đăng kí chat
            services.AddScoped<IChatStreamSender, ChatStreamSender>();
            //chat AI
            services.AddScoped<IPythonApiService, PythonApiService>();
            services.AddScoped<IChatStreamingService, ChatStreamingService>();
            return services;
        }
    }
}

