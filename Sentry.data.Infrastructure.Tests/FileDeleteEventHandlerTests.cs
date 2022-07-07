using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Mock;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class FileDeleteEventHandlerTests
    {
        [TestMethod]
        public void Process_Pending_Delete_Failure_And_Deleted_Sucessfully()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();
            

            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));
            
            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': [
                                        {
                                          'DatasetFileId': 3000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/1/24/00_kb_001_20220124031643505.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/1/24/20220124031643505/00_kb_001.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'Failure'
                                        },
                                        {
                                          'DatasetFileId': 4000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'Success'
                                        },
                                        {
                                          'DatasetFileId': 5000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'unknown'
                                        }
                                      ]
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object,mockDataFileService.Object,mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Once);

        }

        [TestMethod]
        public void Missing_DeleteProcessStatusPerIDProcess()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            
            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);




            string mockMessage2 = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': [
                                        
                                      ]
                                    }
                                    ";
            handle.HandleLogic(mockMessage2);
            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);
        }


        [TestMethod]
        public void Empty_DeleteProcessStatusPerIDProcess()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': [ ]
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);
        }

        [TestMethod]
        public void Bad_DatasetFileIdDeleteStatus()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': [
                                        {
                                          'DatasetFileId': 3000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/1/24/00_kb_001_20220124031643505.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/1/24/20220124031643505/00_kb_001.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatusbad': 'Failure'
                                        },
                                        {
                                          'DatasetFileId': 4000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'Success'
                                        },
                                        {
                                          'DatasetFileId': 5000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'unknown'
                                        }
                                      ]
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Once);
        }

        [TestMethod]
        public void FeatureFlagWorks()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(false);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': 'DeleteProcessStatusPerID': 
                                        [
                                            {
                                              'DatasetFileId': 3000,
                                              'DeletedFiles': [
                                                {
                                                  'bucket': 'sentry-dlst-qual-dataset-ae2',
                                                  'deleteProcessStatus': 'NotFound',
                                                  'fileType': 'RawQuery',
                                                  'key': 'rawquery/DATA/QUAL/3171319/2022/1/24/00_kb_001_20220124031643505.csv'
                                                },
                                                {
                                                  'bucket': 'sentry-dlst-qual-dataset-ae2',
                                                  'deleteProcessStatus': 'NotFound',
                                                  'fileType': 'Raw',
                                                  'key': 'raw/DATA/QUAL/3171319/2022/1/24/20220124031643505/00_kb_001.csv'
                                                },
                                                {
                                                  'bucket': 'sentry-dlst-qual-dataset-ae2',
                                                  'deleteProcessStatus': 'NotFound',
                                                  'fileType': 'Parquet',
                                                  'key': []
                                                }
                                              ],
                                              'DatasetFileIdDeleteStatus': 'Failure'
                                            } 
                                       ]
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
        }


        [TestMethod]
        public void Process_Mutiples_success()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = @"
                                    {
                                      'DeleteProcessStatus': 'Failure',
                                      'EventType': 'FILE_DELETE_RESPONSE',
                                      'RequestGUID': '20220606110640049',
                                      'DeleteProcessStatusPerID': [
                                        {
                                          'DatasetFileId': 3000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/1/24/00_kb_001_20220124031643505.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/1/24/20220124031643505/00_kb_001.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'success'
                                        },
                                        {
                                          'DatasetFileId': 3000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'Success'
                                        },
                                        {
                                          'DatasetFileId': 3000,
                                          'DeletedFiles': [
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'RawQuery',
                                              'key': 'rawquery/DATA/QUAL/3171319/2022/2/16/zzz0101_20220216194839240.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Raw',
                                              'key': 'raw/DATA/QUAL/3171319/2022/2/16/20220216194839240/zzz0101.csv'
                                            },
                                            {
                                              'bucket': 'sentry-dlst-qual-dataset-ae2',
                                              'deleteProcessStatus': 'NotFound',
                                              'fileType': 'Parquet',
                                              'key': []
                                            }
                                          ],
                                          'DatasetFileIdDeleteStatus': 'failure'
                                        }
                                      ]
                                    }
                                    ";
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
        }

    }
}
