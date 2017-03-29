using Sentry.data.Core;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DemoDataController : Controller
    {
        private IDataAssetContext _dataAssetContext;
        private IDataFeedContext _feedContext;
        private AssetDynamicDetailsService _dynamicDetailsService;
        private UserService _userService;

        public DemoDataController(IDataAssetContext dataAssetContext, IDataFeedContext dataFeedContext, AssetDynamicDetailsService dynamicDetailsService, UserService userService)
        {
            _dataAssetContext = dataAssetContext;
            _feedContext = dataFeedContext;
            _dynamicDetailsService = dynamicDetailsService;
            _userService = userService;
        }

        [HttpPost()]
        public void Refresh()
        {
            //sleep is here to be able to see the spinner on the UI
            System.Threading.Thread.Sleep(5000);

            _dataAssetContext.DeleteAllData();
            _dataAssetContext.SaveChanges();

            DomainUser john = new DomainUser("069301") { Ranking = 20 };
            DomainUser evan = new DomainUser("065520") { Ranking = 25 };
            DomainUser darko = new DomainUser("079681") { Ranking = 10 };
            DomainUser tim = new DomainUser("071393") { Ranking = 15 };
            DomainUser cory = new DomainUser("054983") { Ranking = 15 };
            DomainUser sridhar = new DomainUser("072349") { Ranking = 15 };
            DomainUser kristen = new DomainUser("073341") { Ranking = 15 };
            _dataAssetContext.Add(john);
            _dataAssetContext.Add(evan);
            _dataAssetContext.Add(darko);
            _dataAssetContext.Add(tim);
            _dataAssetContext.Add(cory);
            _dataAssetContext.Add(kristen);
            _dataAssetContext.Add(sridhar);
            _dataAssetContext.Add(new DomainUser("067664") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("069508") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("066167") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("076975") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("068144") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("068282") { Ranking = 5 });
            _dataAssetContext.Add(new DomainUser("071559") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("078769") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("077009") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("067915") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("065520") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("073341") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("072814") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("073323") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("067919") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("067816") { Ranking = 5});
            _dataAssetContext.Add(new DomainUser("071393") { Ranking = 5});

            Category databases = new Category("Databases");
            Category dbSQLServer = new Category("SQL Server", databases);
            Category dbOracle = new Category("Oracle", databases);
            Category flatFiles = new Category("Flat Files");
            Category excel = new Category("Antiques", flatFiles);
            Category sas = new Category("SAS", flatFiles);
            Category csv = new Category("Wind-Up Toys", flatFiles);
            _dataAssetContext.Add(databases);
            _dataAssetContext.Add(dbSQLServer);
            _dataAssetContext.Add(dbOracle);
            _dataAssetContext.Add(flatFiles);
            _dataAssetContext.Add(excel);
            _dataAssetContext.Add(sas);
            _dataAssetContext.Add(csv);

            Asset clODS = new Asset("CL ODS", "Commercial Lines ODS");
            _dataAssetContext.Add(clODS);

            Asset plODS = new Asset("PL ODS", "Personal Lines ODS");
            _dataAssetContext.Add(plODS);

            Asset claimODS = new Asset("Claim ODS", "Claims ODS");
            _dataAssetContext.Add(claimODS);

            Asset seraCL = new Asset("SERA CL", "Commercial Lines Analytics");
            _dataAssetContext.Add(seraCL);

            Asset seraPL = new Asset("SERA PL", "Personal Lines Analytics");
            _dataAssetContext.Add(seraPL);

            Asset seraENT = new Asset("SERA ENT", "Enterprise Analytics");
            _dataAssetContext.Add(seraENT);

            Asset pcrCL = new Asset("PCR CL", "Commercial Lines PCR");
            _dataAssetContext.Add(pcrCL);

            Asset pcrPL = new Asset("PCR PL", "Personal Lines PCR");
            _dataAssetContext.Add(pcrPL);

            Asset laser = new Asset("LASER", "Dr. Evil was here");
            _dataAssetContext.Add(laser);

            Asset tdm = new Asset("TDM", "The 'D' followed by the 'M'");
            _dataAssetContext.Add(tdm);

            _dataAssetContext.SaveChanges();

        }
    }
}
