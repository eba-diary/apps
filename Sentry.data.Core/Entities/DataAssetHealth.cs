using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataAssetHealth
    {
        private string _dataAssetName;
        private DateTime _assetUpdtDtm;
        private string _serverName;
        private string _cubeName;
        private string _sourceSystem;
        private DateTime _lastUpdtDtm;
        private int _dataAssetHealthID;

        public DataAssetHealth(
            string dataAssetName,
            DateTime assetUpdtDtm,
            string serverName,
            string cubeName,
            string sourceSystem,
            DateTime lastUpdtDtm,
            int dataAssetHealthID
            )
        {
            this._dataAssetName = dataAssetName;
            this._assetUpdtDtm = assetUpdtDtm;
            this._serverName = serverName;
            this._cubeName = cubeName;
            this._sourceSystem = sourceSystem;
            this._lastUpdtDtm = LastUpdateDTM;
            this._dataAssetHealthID = DataAssetHealthID;
        }

        public int DataAssetHealthID
        {
            get { return _dataAssetHealthID; }
            set { _dataAssetHealthID = value; }
        }

        public DateTime LastUpdateDTM
        {
            get { return _lastUpdtDtm; }
            set { _lastUpdtDtm = value; }
        }

        public string SourceSystem
        {
            get { return _sourceSystem; }
            set { _sourceSystem = value; }
        }

        public string CubeName
        {
            get { return _cubeName; }
            set { _cubeName = value; }
        }

        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        public DateTime AssetUpdtDTM
        {
            get { return _assetUpdtDtm; }
            set { _assetUpdtDtm = value; }
        }

        public string DataAssetName
        {
            get { return _dataAssetName; }
            set { _dataAssetName = value; }
        }

    }
}
