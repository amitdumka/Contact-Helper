using AutoMapper;

using VCard = MixERP.Net.VCards.VCard;

namespace Contact_Helper
{
    internal class AutoMapperHelper
    {

        public AutoMapperHelper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VCard, ContactExt>();
                cfg.CreateMap<ContactExt, VCard>();
            });
            // only during development, validate your mappings; remove it before release
#if DEBUG
            configuration.AssertConfigurationIsValid();
#endif
            this.Configuration = configuration;

            // use DI (http://docs.automapper.org/en/latest/Dependency-injection.html) or create the mapper yourself
            var mapper = configuration.CreateMapper();
            this.Mapper = mapper;
        }

        public MapperConfiguration Configuration { get; set; }
        public IMapper Mapper { get; set; }
    }
}