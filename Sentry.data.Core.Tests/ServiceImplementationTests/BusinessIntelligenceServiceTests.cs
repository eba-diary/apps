using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class BusinessIntelligenceServiceTests : BaseCoreUnitTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }

        [TestMethod]
        [Ignore]
        public void Get_Business_Intelligence_Dto_By_Id()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.GetById<Dataset>(1)).Return(BuildMockDataset().First()).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            daContext.Stub(x => x.Users).Return(BuildMockDomainUsers()).Repeat.Any();

            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            extUsrInfoProvider.Stub(x => x.GetByUserId("081226")).Return(BuildMockExtendedUserInfo()).Repeat.Any();

            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDto bi = biSrvc.GetBusinessIntelligenceDto(1);

            Assert.IsNotNull(bi);
            Assert.AreEqual("My Mock Dataset", bi.DatasetName);
            Assert.AreEqual("RPT", bi.DatasetType);
            Assert.AreEqual(1, bi.DatasetFunctionIds.Count);
            Assert.AreEqual(1, bi.TagIds.Count);
            Assert.AreEqual("James Gordon", bi.PrimaryContactName);
            Assert.IsTrue(bi.MailtoLink.StartsWith("mailto:?Subject=Business"));
        }

        [TestMethod]
        [Ignore]
        public void Get_Business_Intelligence_Detail_Dto_By_Id()
        {
            

            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.GetById<Dataset>(1)).Return(BuildMockDataset().First()).Repeat.Any();
            dsContext.Stub(x => x.IsUserSubscribedToDataset("081226", 1)).Return(true).Repeat.Any();
            dsContext.Stub(x => x.GetAllUserSubscriptionsForDataset("081226", 1)).Return(BuildMockDatasetSubscriptions()).Repeat.Any();
            dsContext.Stub(x => x.Events).Return(BuildMockEvents(1)).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            daContext.Stub(x => x.Users).Return(BuildMockDomainUsers()).Repeat.Any();

            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            extUsrInfoProvider.Stub(x => x.GetByUserId("081226")).Return(BuildMockExtendedUserInfo()).Repeat.Any();

            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            currUsrIdProvider.Stub(x => x.GetImpersonatedUserId()).Return("").Repeat.Any();
            currUsrIdProvider.Stub(x => x.GetRealUserId()).Return("081226").Repeat.Any();

            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDetailDto biDtl = biSrvc.GetBusinessIntelligenceDetailDto(1);

            Assert.IsNotNull(biDtl);
            Assert.AreEqual("My Mock Dataset", biDtl.DatasetName);
            Assert.AreEqual(1, biDtl.AmountOfSubscriptions);
            Assert.AreEqual("plum", biDtl.CategoryColor);
            Assert.AreEqual(1, biDtl.CategoryNames.Count);
            Assert.AreEqual("IT", biDtl.CategoryNames[0]);
            Assert.AreEqual(1, biDtl.TagNames.Count);
            Assert.AreEqual("Catastrophe", biDtl.TagNames[0]);
        }

        [TestMethod]
        public void Get_Home_Dto()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.GetReportCount()).Return(7).Repeat.Any();
            dsContext.Stub(x => x.Categories).Return(BuildMockCategories()).Repeat.Any();

            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            extUsrInfoProvider.Stub(x => x.GetByUserId("081226")).Return(BuildMockExtendedUserInfo()).Repeat.Any();

            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            currUsrIdProvider.Stub(x => x.GetImpersonatedUserId()).Return("").Repeat.Any();
            currUsrIdProvider.Stub(x => x.GetRealUserId()).Return("081226").Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceHomeDto biHome = biSrvc.GetHomeDto();

            Assert.IsNotNull(biHome);
            Assert.IsTrue(biHome.CanManageReports);
            Assert.IsFalse(biHome.CanEditDataset);
            Assert.IsFalse(biHome.CanUpload);
            Assert.AreEqual(6, biHome.Categories.Count);
            Assert.IsFalse(biHome.Categories.Any(x => x.ObjectType == GlobalConstants.DataEntityCodes.DATASET));
            Assert.AreEqual(7, biHome.DatasetCount);
        }

        [TestMethod]
        public void Get_User_Security_By_Dataset_Id()
        {
            List<Dataset> datasets = new List<Dataset>
            {
                BuildMockDataset().First()
            };

            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.Datasets).Return(datasets.AsQueryable()).Repeat.Any();
            dsContext.Stub(x => x.Security).Return(BuildMockDatasetSecurity()).Repeat.Any();
            dsContext.Stub(x => x.SecurityTicket).Return(BuildMockSecurityTickets()).Repeat.Any();

            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            extUsrInfoProvider.Stub(x => x.GetByUserId("081226")).Return(BuildMockExtendedUserInfo()).Repeat.Any();

            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            currUsrIdProvider.Stub(x => x.GetImpersonatedUserId()).Return("").Repeat.Any();
            currUsrIdProvider.Stub(x => x.GetRealUserId()).Return("081226").Repeat.Any();

            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            securitySrvc.Stub(x => x.GetUserSecurity(Arg<Dataset>.Is.Anything, Arg<IApplicationUser>.Is.Anything)).Return(BuildMockUserSecurity()).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(securitySrvc);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            UserSecurity usrSec = biSrvc.GetUserSecurityById(1);

            Assert.IsNotNull(usrSec);
            Assert.IsTrue(usrSec.CanCreateDataset);
            Assert.IsTrue(usrSec.CanCreateReport);
            Assert.IsTrue(usrSec.CanEditDataset);
            Assert.IsTrue(usrSec.CanEditReport);
            Assert.IsTrue(usrSec.CanPreviewDataset);
            Assert.IsTrue(usrSec.CanQueryDataset);
            Assert.IsFalse(usrSec.CanUploadToDataset);
            Assert.IsTrue(usrSec.CanViewFullDataset);
        }

        [TestMethod]
        public void Validate_Dataset_No_Errors()
        {
            List<Dataset> datasets = new List<Dataset>
            {
                BuildMockDataset().First()
            };

            SetupBusinessIntelligenceServiceForValidations(datasets.AsQueryable());

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDto dto = new BusinessIntelligenceDto
            {
                DatasetId = 0,
                DatasetName = "My New IT Mock Dataset",
                DatasetCategoryIds = new List<int> { 1 },
                DatasetType = GlobalConstants.DataEntityCodes.REPORT,
                FileTypeId = (int)ReportType.Tableau
            };

            List<string> errors = biSrvc.Validate(dto);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void Validate_Dataset_Name_Already_Exists_Within_Category()
        {
            List<Dataset> datasets = new List<Dataset>
            {
                BuildMockDataset().First()
            };

            SetupBusinessIntelligenceServiceForValidations(datasets.AsQueryable());

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDto dto = new BusinessIntelligenceDto
            {
                DatasetId = 0,
                DatasetName = "My Mock Dataset",
                DatasetCategoryIds = new List<int> { 1 },
                DatasetType = GlobalConstants.DataEntityCodes.REPORT
            };

            List<string> errors = biSrvc.Validate(dto);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Dataset name already exists within category", errors[0]);
        }

        [TestMethod]
        public void Validate_Dataset_Error_Finding_Excel_File()
        {
            List<Dataset> datasets = new List<Dataset>
            {
                BuildMockDataset().First()
            };

            SetupBusinessIntelligenceServiceForValidations(datasets.AsQueryable());

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDto dto = new BusinessIntelligenceDto
            {
                DatasetId = 0,
                DatasetName = "My New Dataset",
                DatasetCategoryIds = new List<int> { 1 },
                DatasetType = GlobalConstants.DataEntityCodes.REPORT,
                Location = "\\\\Sentry.com\\Share\\S_Share\\Folder_Path\\MyFolder\\MyFile.xlsx",
                FileTypeId = (int)ReportType.Excel
            };

            List<string> errors = biSrvc.Validate(dto);

            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("An error occured finding the file. Please verify the file path is correct or contact DSCSupport@sentry.com for assistance.", errors[0]);
        }

        [TestMethod]
        public void Create_Dataset_Success()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.Categories).Return(BuildMockCategories()).Repeat.Any();
            dsContext.Stub(x => x.BusinessUnits).Return(BuildMockBusinessUnits()).Repeat.Any();
            dsContext.Stub(x => x.DatasetFunctions).Return(BuildMockDatasetFunctions()).Repeat.Any();
            dsContext.Stub(x => x.Tags).Return(BuildMockMetadataTags()).Repeat.Any();
            dsContext.Stub(x => x.DatasetScopeTypes).Return(BuildMockDatasetScopeTypes()).Repeat.Any();
            dsContext.Stub(x => x.FileExtensions).Return(BuildMockFileExtensions()).Repeat.Any();
            dsContext.Stub(x => x.Datasets).Return(BuildMockDataset()).Repeat.Any();
            dsContext.Stub(x => x.Assets).Return(BuildMockAsset()).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            bool datasetSaved = biSrvc.CreateAndSaveBusinessIntelligence(BuildMockBusinessIntelligenceDto());

            Assert.IsTrue(datasetSaved);
        }

        [TestMethod]
        public void Create_Dataset_Failure()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.Categories).Return(BuildMockCategories()).Repeat.Any();
            dsContext.Stub(x => x.BusinessUnits).Return(BuildMockBusinessUnits()).Repeat.Any();
            dsContext.Stub(x => x.DatasetFunctions).Return(BuildMockDatasetFunctions()).Repeat.Any();
            dsContext.Stub(x => x.Tags).Return(BuildMockMetadataTags()).Repeat.Any();
            dsContext.Stub(x => x.DatasetScopeTypes).Return(BuildMockDatasetScopeTypes()).Repeat.Any();
            dsContext.Stub(x => x.FileExtensions).Return(BuildMockFileExtensions()).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();


            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            BusinessIntelligenceDto dto = BuildMockBusinessIntelligenceDto();
            dto.DatasetBusinessUnitIds = null;

            bool datasetSaved = biSrvc.CreateAndSaveBusinessIntelligence(dto);

            Assert.IsFalse(datasetSaved);
        }

        [TestMethod]
        public void Delete_Dataset()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.GetById<Dataset>(1)).Return(BuildMockDataset().First()).Repeat.Any();

            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(securitySrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(s3ServiceProvider);

            var biSrvc = _container.GetInstance<IBusinessIntelligenceService>();

            biSrvc.Delete(1);
        }

        private IQueryable<Dataset> BuildMockDataset()
        {
            Dataset newDs = new Dataset()
            {
                DatasetId = 1,
                PrimaryContactId = "081226",
                UploadUserName = "081226",
                IsSecured = false,
                DatasetCategories = new List<Category> {
                        new Category
                        {
                            Id = 1,
                            Color = "plum",
                            Name = "IT",
                            AbbreviatedName = "IT",
                            ObjectType = "RPT"
                        }
                    },
                BusinessUnits = new List<BusinessUnit>
                    {
                        new BusinessUnit
                        {
                            Id = 1,
                            Name = "Direct Writer",
                            AbbreviatedName = "DW",
                            Sequence = 1
                        }
                    },
                DatasetFunctions = new List<DatasetFunction>
                    {
                        new DatasetFunction
                        {
                            Id = 1,
                            Name = "Operations",
                            Sequence = 2
                        }
                    },
                DatasetName = "My Mock Dataset",
                DatasetDesc = "Mock dataset for the purpose of unit testing",
                CreationUserName = "081226",
                DatasetType = GlobalConstants.DataEntityCodes.REPORT,
                Metadata = new DatasetMetadata
                {
                    ReportMetadata = new ReportMetadata
                    {
                        Location = "https://tableau.sentry.com/#/site/SentryInsurance/views/WorkflowPayloads/WorkflowPayloads?:iid=1",
                        Frequency = 1,
                        GetLatest = false,
                        LocationType = "https"
                    }
                },
                Tags = new List<MetadataTag>
                    {
                        new MetadataTag
                        {
                            TagId = 1,
                            Name = "Catastrophe",
                            Created = DateTime.Today,
                            CreatedBy = "081226",
                            Description = "A catastrophe occurred",
                            Group = new TagGroup
                            {
                                TagGroupId = 6,
                                Description = "General measures",
                                Created = DateTime.Today,
                                Name = "Measures"
                            }
                        }
                    },
                DatasetFileConfigs = new List<DatasetFileConfig>
                    {
                        new DatasetFileConfig
                        {
                            ConfigId = 60,
                            FileTypeId = 3,
                            Name = "Sample Config File Name"
                        }
                    },
                CanDisplay = true,
                Favorities = new List<Favorite>
                    {
                        new Favorite
                        {
                            DatasetId = 1,
                            Created = DateTime.Today.AddDays(-12),
                            FavoriteId = 1,
                            Sequence = 1,
                            UserId = "081226"
                        }
                    },
                Security = BuildMockDatasetSecurity().ToList()[0],                
            };

            newDs.Images = new List<Image>
                {
                    new Image
                    {
                        ImageId = 1,
                        FileName = "Image1.jpg",
                        FileExtension = "jpg",
                        Sort = 1,
                        ParentDataset = newDs
                    }
                };

            List<Dataset> ds = new List<Dataset>
            {
                newDs
            };

            return ds.AsQueryable();
        }

        private IQueryable<Asset> BuildMockAsset()
        {
            Asset newDs = new Asset()
            {
                AssetId = 1,
                SaidKeyCode = "DATA",
                Security = BuildMockDatasetSecurity().ToList()[0],
            };

            List<Asset> ds = new List<Asset>
            {
                newDs
            };

            return ds.AsQueryable();
        }

        private IExtendedUserInfo BuildMockExtendedUserInfo()
        {
            IExtendedUserInfo extUsrInfo = MockRepository.GenerateMock<IExtendedUserInfo>();
            extUsrInfo.Stub(x => x.UserId).Return("081226").Repeat.Any();
            extUsrInfo.Stub(x => x.FirstName).Return("James").Repeat.Any();
            extUsrInfo.Stub(x => x.LastName).Return("Gordon").Repeat.Any();
            extUsrInfo.Stub(x => x.EmailAddress).Return("james.gordon@gcpd.com").Repeat.Any();
            extUsrInfo.Stub(x => x.Permissions).Return(BuildMockPermissions()).Repeat.Any();

            return extUsrInfo;
        }

        private IQueryable<DomainUser> BuildMockDomainUsers()
        {
            List<DomainUser> domUsers = new List<DomainUser>
            {
                new DomainUser("081226")
                {
                    Ranking = 1
                }
            };

            return domUsers.AsQueryable();
        }

        private IQueryable<Category> BuildMockCategories()
        {
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Claim",
                    Color = "blueGray",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 2,
                    Name = "Industry",
                    Color = "orange",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 3,
                    Name = "Government",
                    Color = "gold",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 4,
                    Name = "Geographic",
                    Color = "green",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 5,
                    Name = "Weather",
                    Color = "plum",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 6,
                    Name = "Sentry",
                    Color = "blue",
                    ObjectType = "DS"
                },
                new Category
                {
                    Id = 7,
                    Name = "Commercial Lines",
                    Color = "blueGray",
                    ObjectType = "RPT",
                    AbbreviatedName = "CL"
                },
                new Category
                {
                    Id = 8,
                    Name = "Personal Lines",
                    Color = "orange",
                    ObjectType = "RPT",
                    AbbreviatedName = "PL"
                },
                new Category
                {
                    Id = 9,
                    Name = "Claims",
                    Color = "gold",
                    ObjectType = "RPT"
                },
                new Category
                {
                    Id = 10,
                    Name = "Corporate",
                    Color = "green",
                    ObjectType = "RPT"
                },
                new Category
                {
                    Id = 11,
                    Name = "IT",
                    Color = "green",
                    ObjectType = "RPT"
                },
                new Category
                {
                    Id = 12,
                    Name = "401k",
                    Color = "blue",
                    ObjectType = "RPT"
                }
            };

            return categories.AsQueryable();
        }

        private IQueryable<BusinessUnit> BuildMockBusinessUnits()
        {
            List<BusinessUnit> bizUnits = new List<BusinessUnit>
            {
                new BusinessUnit
                {
                    Id = 1,
                    Name = "Direct Writer",
                    AbbreviatedName = "DW",
                    Sequence = 1
                },
                new BusinessUnit
                {
                    Id = 2,
                    Name = "National Accounts",
                    AbbreviatedName = "NA",
                    Sequence = 2
                },
                new BusinessUnit
                {
                    Id = 3,
                    Name = "Regional",
                    AbbreviatedName = "RG",
                    Sequence = 3
                },
                new BusinessUnit
                {
                    Id = 4,
                    Name = "Transportation",
                    AbbreviatedName = "TR",
                    Sequence = 4
                },
                new BusinessUnit
                {
                    Id = 5,
                    Name = "Hortica",
                    AbbreviatedName = "NRT",
                    Sequence = 5
                },
                new BusinessUnit
                {
                    Id = 6,
                    Name = "Life and Health",
                    AbbreviatedName = "LH",
                    Sequence = 6
                }
            };

            return bizUnits.AsQueryable();
        }

        private IQueryable<DatasetFunction> BuildMockDatasetFunctions()
        {
            List<DatasetFunction> dsFuncs = new List<DatasetFunction>
            {
                new DatasetFunction
                {
                    Id = 1,
                    Name = "Management",
                    Sequence = 1
                },
                new DatasetFunction
                {
                    Id = 2,
                    Name = "Operations",
                    Sequence = 2
                },
                new DatasetFunction
                {
                    Id = 3,
                    Name = "PL State Management",
                    Sequence = 3
                },
                new DatasetFunction
                {
                    Id = 4,
                    Name = "Pricing",
                    Sequence = 4
                },
                new DatasetFunction
                {
                    Id = 5,
                    Name = "Sales",
                    Sequence = 5
                },
                new DatasetFunction
                {
                    Id = 6,
                    Name = "Underwriting",
                    Sequence = 6
                }
            };

            return dsFuncs.AsQueryable();
        }

        private IEnumerable<string> BuildMockPermissions()
        {
            List<string> permissions = new List<string>
            {
                GlobalConstants.PermissionCodes.REPORT_MODIFY
            };

            return permissions.AsEnumerable();
        }

        private List<DatasetSubscription> BuildMockDatasetSubscriptions()
        {
            return new List<DatasetSubscription>
            {
                new DatasetSubscription
                {
                    ID = 1,
                    SentryOwnerName = "081226"
                }
            };
        }

        private IQueryable<Event> BuildMockEvents(int datasetId)
        {
            List<Event> events = new List<Event>
            {
                new Event
                {
                    EventType = new EventType
                    {
                        Description = GlobalConstants.EventType.VIEWED
                    },
                    Dataset = datasetId
                }
            };

            return events.AsQueryable();
        }

        private UserSecurity BuildMockUserSecurity()
        {
            return new UserSecurity
            {
                CanCreateDataset = true,
                CanCreateReport = true,
                CanEditDataset = true,
                CanEditReport = true,
                CanPreviewDataset = true,
                CanQueryDataset = true,
                CanUploadToDataset = false,
                CanViewFullDataset = true
            };
        }

        private IQueryable<Security> BuildMockDatasetSecurity()
        {
            List<Security> security = new List<Security>
            {
                new Security
                {
                    SecurityId = new Guid("16FBB09F-52E7-4DA7-9724-A9EB011824AE"),
                    SecurableEntityName = "Dataset",
                    CreatedDate = DateTime.Today.AddDays(-2),
                    CreatedById = "081226",
                    EnabledDate = DateTime.Today.AddDays(-2),
                    Tickets = new List<SecurityTicket>
                    {
                        BuildMockSecurityTicket()
                    }
                }
            };

            return security.AsQueryable();
        }

        private IQueryable<SecurityTicket> BuildMockSecurityTickets()
        {
            List<SecurityTicket> tickets = new List<SecurityTicket>
            {
                BuildMockSecurityTicket()
            };

            return tickets.AsQueryable();
        }

        private SecurityTicket BuildMockSecurityTicket()
        {
            return new SecurityTicket
            {
                SecurityTicketId = new Guid("23441492-1570-46A0-BC25-A9EC0096DE2D"),
                TicketId = "C00533971",
                RequestedById = "081226",
                ApprovedById = "078193",
                RequestedDate = DateTime.Today.AddDays(-5),
                ApprovedDate = DateTime.Today,
                TicketStatus = GlobalConstants.ChangeTicketStatus.COMPLETED,
                AdGroupName = "DDA_DataArchNP_DB",
                IsRemovingPermission = false,
                IsAddingPermission = true,
                ParentSecurity = new Security
                {
                    SecurityId = new Guid("16FBB09F-52E7-4DA7-9724-A9EB011824AE")
                },
                AddedPermissions = BuildMockSecurityPermissions()
            };
        }

        private List<SecurityPermission> BuildMockSecurityPermissions()
        {
            return new List<SecurityPermission>
            {
                new SecurityPermission
                {
                    SecurityPermissionId = new Guid("8C175C01-69EB-4D2E-8C2F-A9EC0096DE30"),
                    IsEnabled = true,
                    AddedDate = DateTime.Today,
                    EnabledDate = DateTime.Today,
                    AddedFromTicket = new SecurityTicket
                    {
                        SecurityTicketId = new Guid("23441492-1570-46A0-BC25-A9EC0096DE2D")
                    },
                    Permission = new Permission
                    {
                        PermissionId = 1,
                        PermissionCode = "CanPreviewDataset",
                        PermissionName = "Preview Dataset",
                        PermissionDescription = "Access to dataset metadata",
                        SecurableObject = "Dataset"
                    }
                },
                new SecurityPermission
                {
                    SecurityPermissionId = new Guid("9A721B21-5B58-4ACE-9049-A9EC0096DE30"),
                    IsEnabled = true,
                    AddedDate = DateTime.Today,
                    EnabledDate = DateTime.Today,
                    AddedFromTicket = new SecurityTicket
                    {
                        SecurityTicketId = new Guid("23441492-1570-46A0-BC25-A9EC0096DE2D")
                    },
                    Permission = new Permission
                    {
                        PermissionId = 2,
                        PermissionCode = "CanViewFullDataset",
                        PermissionName = "View Full Dataset",
                        PermissionDescription = "Access to full dataset with download capability",
                        SecurableObject = "Dataset"
                    }
                },
                new SecurityPermission
                {
                    SecurityPermissionId = new Guid("E40F7EDB-95E7-41B0-AC19-A9EC0096DE30"),
                    IsEnabled = true,
                    AddedDate = DateTime.Today,
                    EnabledDate = DateTime.Today,
                    AddedFromTicket = new SecurityTicket
                    {
                        SecurityTicketId = new Guid("23441492-1570-46A0-BC25-A9EC0096DE2D")
                    },
                    Permission = new Permission
                    {
                        PermissionId = 3,
                        PermissionCode = "CanUploadToDataset",
                        PermissionName = "Upload to Dataset",
                        PermissionDescription = "Access to upload data files to dataset",
                        SecurableObject = "Dataset"
                    }
                },
                new SecurityPermission
                {
                    SecurityPermissionId = new Guid("05436D5B-E5D2-4C34-B8FA-A9EC0096DE30"),
                    IsEnabled = true,
                    AddedDate = DateTime.Today,
                    EnabledDate = DateTime.Today,
                    AddedFromTicket = new SecurityTicket
                    {
                        SecurityTicketId = new Guid("23441492-1570-46A0-BC25-A9EC0096DE2D")
                    },
                    Permission = new Permission
                    {
                        PermissionId = 4,
                        PermissionCode = "CanQueryDataset",
                        PermissionName = "Query Dataset",
                        PermissionDescription = "Access to query dataset using the Query Tool",
                        SecurableObject = "Dataset"
                    }
                }
            };
        }

        private void SetupBusinessIntelligenceServiceForValidations(IQueryable<Dataset> datasets)
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.Datasets).Return(datasets).Repeat.Any();

            IExtendedUserInfoProvider extUsrInfoProvider = MockRepository.GenerateMock<IExtendedUserInfoProvider>();
            ICurrentUserIdProvider currUsrIdProvider = MockRepository.GenerateMock<ICurrentUserIdProvider>();
            ISecurityService securitySrvc = MockRepository.GenerateMock<ISecurityService>();
            IDataAssetContext daContext = MockRepository.GenerateMock<IDataAssetContext>();
            IEmailService emailSrvc = MockRepository.GenerateMock<IEmailService>();
            IS3ServiceProvider s3ServiceProvider = MockRepository.GenerateMock<IS3ServiceProvider>();

            _container.Inject(dsContext);
            _container.Inject(emailSrvc);
            _container.Inject(daContext);
            _container.Inject(extUsrInfoProvider);
            _container.Inject(currUsrIdProvider);
            _container.Inject(securitySrvc);
            _container.Inject(s3ServiceProvider);
        }

        private BusinessIntelligenceDto BuildMockBusinessIntelligenceDto()
        {
            return new BusinessIntelligenceDto
            {
                AmountOfSubscriptions = 2,
                CanDisplay = true,
                CategoryColor = "plum",
                DatasetCategoryIds = new List<int> { 1 },
                DatasetBusinessUnitIds = new List<int> { 1, 3 },
                DatasetFunctionIds = new List<int> { 2, 4, 5 },
                TagIds = new List<string> { "1" },
                Images = new List<ImageDto>()
                {
                    new ImageDto()
                    {
                        DatasetId = 0,
                        DeleteImage = false,
                        FileName = "NewImageFile.jpg",
                        sortOrder = 1
                    }
                }
            };
        }



        private IQueryable<MetadataTag> BuildMockMetadataTags()
        {
            List<MetadataTag> tags = new List<MetadataTag>
            {
                new MetadataTag
                {
                    TagId = 1,
                    Name = "Premium",
                    Created = DateTime.Today.AddDays(-10)
                },
                new MetadataTag
                {
                    TagId = 2,
                    Name = "Loss",
                    Created = DateTime.Today.AddDays(-8)
                }
            };

            return tags.AsQueryable();
        }

        private IQueryable<DatasetScopeType> BuildMockDatasetScopeTypes()
        {
            List<DatasetScopeType> scopes = new List<DatasetScopeType>
            {
                new DatasetScopeType
                {
                    ScopeTypeId = 1,
                    Name = "Point-in-Time",
                    Description = "Datafiles are transactional in nature. Data is not repeated across files.",
                    IsEnabled = true
                },
                new DatasetScopeType
                {
                    ScopeTypeId = 2,
                    Name = "Appending",
                    Description = "New data is appened to datafile. Data has potential to be repeated across files.",
                    IsEnabled = true
                },
                new DatasetScopeType
                {
                    ScopeTypeId = 3,
                    Name = "Floating-Window",
                    Description = "Datafile contains data over a defined period of time. As new data is added, oldest data is dropped off. Data is repeated across files.",
                    IsEnabled = true
                }
            };

            return scopes.AsQueryable();
        }

        private IQueryable<FileExtension> BuildMockFileExtensions()
        {
            List<FileExtension> fleExt = new List<FileExtension>
            {
                new FileExtension
                {
                    Id = 1,
                    Name = GlobalConstants.ExtensionNames.JSON,
                    Created = DateTime.Today.AddDays(-22),
                    CreatedUser = "096644"
                },
                new FileExtension
                {
                    Id = 2,
                    Name = GlobalConstants.ExtensionNames.CSV,
                    Created = DateTime.Today.AddDays(-38),
                    CreatedUser = "096644"
                },
                new FileExtension
                {
                    Id = 3,
                    Name = GlobalConstants.ExtensionNames.TXT,
                    Created = DateTime.Today.AddDays(-32),
                    CreatedUser = "096644"
                },
                new FileExtension
                {
                    Id = 4,
                    Name = GlobalConstants.ExtensionNames.XML,
                    Created = DateTime.Today.AddDays(-9),
                    CreatedUser = "096644"
                }
            };

            return fleExt.AsQueryable();
        }
    }
}