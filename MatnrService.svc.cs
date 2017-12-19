using System;
using System.ServiceModel.Activation;
using NLog;

namespace X5.SharePoint.Quality.WebServices
{
    public class MatnrService : si_sharepoint_matnr_inb
    {
        public void si_sharepoint_matnr_inb(si_sharepoint_matnr_inb1 request)
        {
            /// веб-сервис для парсинга справочников PLU
            try
            {
                foreach (var items in request.mt_sharepoint_matnr)
                {
                    foreach (var item in items)
                    {
                        // TODO распарсить item 
                        var plu = new PLUDataItem()
                        {
                            COUNTRY = item.COUNTRY,
                            DESCRIPTION = item.DESCRIPTION,
                            EAN = item.EAN,
                            EXCISE_ITEM = item.EXCISE_ITEM,
                            GROUP = item.GROUP,
                            ID_ITEM = item.ID_ITEM,
                            LICENSED = item.LICENSED,
                            MANUFACTURER_ALC_CODE = item.MANUFACTURER_ALC_CODE,
                            MANUFACTURER_ALC_INN = item.MANUFACTURER_ALC_INN,
                            MANUFACTURER_CODE = item.MANUFACTURER_CODE,
                            PRICE_SEGMENT = item.PRICE_SEGMENT,
                            STM = item.STM,
                            TOTAL_STORAGE_TIME = item.TOTAL_STORAGE_TIME,
                            TYPE_ITEM = item.TYPE_ITEM,
                            UOM = item.UOM,
                            ASSORT_GROUP = item.ASSORT_GROUP
                        };
                        plu.UpdateData(SettingsManager.GetConnectionString());
                    }
                }

            }
            catch (Exception e)
            {
                var log = LogManager.GetCurrentClassLogger();
                log.Error(e);
                throw;
            }
        }
    }
}
