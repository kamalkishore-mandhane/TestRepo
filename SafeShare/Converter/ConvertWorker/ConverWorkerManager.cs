using SafeShare.Converter.ConvertUtil;
using SafeShare.WPFUI.Utils;
using SafeShare.WPFUI.View;
using SafeShare.WPFUI.ViewModel;
using System;
using System.Collections.Generic;

namespace SafeShare.Converter.ConvertWorker
{
    public enum ConvertId
    {
        ReducePhoto,
        RemovePersonalData,
        ConvertToPDF,
        Watermark,
        CombinePDF
    }

    internal class ConverWorkerManager
    {
        private const double ConvertEndProgress = 50.00;

        private Dictionary<ConvertId, bool> _converterDic = new Dictionary<ConvertId, bool>(5);
        private static List<int> _metadataOptions = new List<int>();

        private List<string> _srcFileList;
        private List<string> _originalFileList;
        private ConvertParam _convertParam;
        private int _progressIndex;
        private int _totalProgress;
        private string _destinationDir;
        private IntPtr _parent;

        private List<string> _skipConvertButZipFileList = new List<string>();

        public ConverWorkerManager(ConvertParam param, int progress, int totalProgress)
        {
            _convertParam = param;
            _progressIndex = progress;
            _totalProgress = totalProgress;

            InitConverterDic();
        }

        private void InitConverterDic()
        {
            _converterDic.Add(ConvertId.ReducePhoto, false);
            _converterDic.Add(ConvertId.RemovePersonalData, false);
            _converterDic.Add(ConvertId.ConvertToPDF, false);
            _converterDic.Add(ConvertId.Watermark, false);
            _converterDic.Add(ConvertId.CombinePDF, false);
            TrackHelper.TrackHelperInstance.ConvertIds.Clear();
        }

        public bool NotShowConvertErrorAgain
        {
            get;
            set;
        }

        public CONVERT_ERROR_DIALOG ConvertErrorChoice
        {
            get;
            set;
        }

        public ConvertParam ConvertParams
        {
            get
            {
                return _convertParam;
            }
        }

        public void SetSelectedConverter(ConvertId id)
        {
            if (_converterDic.ContainsKey(id))
            {
                _converterDic[id] = true;
            }
        }

        public void SetSourceFiles(List<string> files)
        {
            _srcFileList = files;
        }

        public void SetOrigianlFiles(List<string> files)
        {
            _originalFileList = files;
        }

        public void SetOutputDirectory(string dir)
        {
            _destinationDir = dir;
        }

        public void SetParentHandle(IntPtr parent)
        {
            _parent = parent;
        }

        public void StartConvert()
        {
            TrackHelper.TrackHelperInstance.ConvertIds.Clear();

            foreach (var item in _converterDic)
            {
                if (item.Value)
                {
                    switch (item.Key)
                    {
                        case ConvertId.ReducePhoto:
                            TrackHelper.TrackHelperInstance.ConvertIds.Add(WzSvcProviderIDs.SPID_IMAGE_RESIZE);
                            DoReducePhoto();
                            if (ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                            {
                                return;
                            }
                            break;

                        case ConvertId.RemovePersonalData:
                            TrackHelper.TrackHelperInstance.ConvertIds.Add(WzSvcProviderIDs.SPID_REMOVEPERSONALDATA_TRANSFORM);
                            DoRemovePersonalData();
                            if (ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                            {
                                return;
                            }
                            break;

                        case ConvertId.ConvertToPDF:
                            TrackHelper.TrackHelperInstance.ConvertIds.Add(WzSvcProviderIDs.SPID_DOC2PDF_TRANSFORM);
                            DoConvertToPDF();
                            if (ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                            {
                                return;
                            }
                            break;

                        case ConvertId.Watermark:
                            TrackHelper.TrackHelperInstance.ConvertIds.Add(WzSvcProviderIDs.SPID_WATERMARK_TRANSFORM);
                            DoWaterMark();
                            if (ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                            {
                                return;
                            }
                            break;

                        case ConvertId.CombinePDF:
                            TrackHelper.TrackHelperInstance.ConvertIds.Add(WzSvcProviderIDs.SPID_COMBINE_PDF_TRANSFORM);
                            DoCombinePDF();
                            if (ConvertErrorChoice == CONVERT_ERROR_DIALOG.Cancel)
                            {
                                return;
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            foreach (var file in _skipConvertButZipFileList)
            {
                if (!_srcFileList.Contains(file))
                {
                    _srcFileList.Add(file);
                }
            }

            TrackHelper.LogShareConversionEvent();
        }

        private void DoReducePhoto()
        {
            var convertOptPageDataContext = _convertParam.COptModel;

            if (convertOptPageDataContext != null)
            {
                Func<int, bool> progressFunc = progressIndex =>
                {
                    _progressIndex = progressIndex;
                    double percent = (double)_progressIndex / (double)_totalProgress * ConvertEndProgress;
                    int progress = Convert.ToInt32(percent);

                    _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                    {
                        _convertParam.experiencePage.Progress.Value = progress;
                    }));

                    return true;
                };

                var worker = new ReducePhotoWorker(this, _srcFileList, _destinationDir, progressFunc);
                worker.Parent = _parent;
                worker.ProgressIndex = _progressIndex;
                worker.Width = convertOptPageDataContext.ReducePhotoWidth;
                worker.Height = convertOptPageDataContext.ReducePhotosHeight;

                worker.Transform();
                GetSkipConvertButZipedFiles(worker);
            }
        }

        private void DoRemovePersonalData()
        {
            var convertOptPageDataContext = _convertParam.COptModel;

            if (convertOptPageDataContext != null)
            {
                InitMetadataOptions();

                Func<int, bool> progressFunc = progressIndex =>
                {
                    _progressIndex = progressIndex;
                    double percent = (double)_progressIndex / (double)_totalProgress * ConvertEndProgress;
                    int progress = Convert.ToInt32(percent);

                    _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                    {
                        _convertParam.experiencePage.Progress.Value = progress;
                    }));

                    return true;
                };

                var worker = new RemovePersonalDataWorker(this, _srcFileList, _destinationDir, progressFunc);
                worker.Parent = _parent;
                worker.ProgressIndex = _progressIndex;
                worker.MetadataOptions = _metadataOptions;

                worker.Transform();
                GetSkipConvertButZipedFiles(worker);
            }
        }

        private void DoConvertToPDF()
        {
            var convertOptPageDataContext = _convertParam.COptModel;

            if (convertOptPageDataContext != null)
            {
                Func<int, bool> progressFunc = progressIndex =>
                {
                    _progressIndex = progressIndex;
                    double percent = (double)_progressIndex / (double)_totalProgress * ConvertEndProgress;
                    int progress = Convert.ToInt32(percent);

                    _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                    {
                        _convertParam.experiencePage.Progress.Value = progress;
                    }));

                    return true;
                };

                var worker = new ConvertToPDFWorker(this, _srcFileList, _destinationDir, progressFunc);
                worker.Parent = _parent;
                worker.ProgressIndex = _progressIndex;
                worker.Resolution = convertOptPageDataContext.Resolution;
                worker.Quality = convertOptPageDataContext.Quality;
                worker.MakePdfReadonly = convertOptPageDataContext.MakePdfReadonly;
                worker.RemoveComments = convertOptPageDataContext.RemoveCommentsIsChecked;

                worker.Transform();
                GetSkipConvertButZipedFiles(worker);
            }
        }

        private void DoWaterMark()
        {
            var convertOptPageDataContext = _convertParam.COptModel;
            if (convertOptPageDataContext != null)
            {
                Func<int, bool> progressFunc = progressIndex =>
                {
                    _progressIndex = progressIndex;
                    double percent = (double)_progressIndex / (double)_totalProgress * ConvertEndProgress;
                    int progress = Convert.ToInt32(percent);

                    _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                    {
                        _convertParam.experiencePage.Progress.Value = progress;
                    }));

                    return true;
                };

                var worker = new WatermarkWorker(this, _srcFileList, _destinationDir, progressFunc);
                worker.Parent = _parent;
                worker.ProgressIndex = _progressIndex;
                worker.TimeStamp = convertOptPageDataContext.TimeStampIsChecked;
                worker.DateStamp = convertOptPageDataContext.DateStampIsChecked;
                worker.WatermarkContent = convertOptPageDataContext.Content;
                worker.TextAngle = convertOptPageDataContext.TextAngle;
                worker.TextPosition = convertOptPageDataContext.TextPosition;
                worker.TextColor = convertOptPageDataContext.TextColor;
                worker.TextOpacity = convertOptPageDataContext.RealTextOpacity;

                worker.Transform();
                GetSkipConvertButZipedFiles(worker);
            }
        }

        private void DoCombinePDF()
        {
            try
            {
                var convertOptPageDataContext = _convertParam.COptModel;
                if (convertOptPageDataContext != null)
                {
                    Func<int, bool> progressFunc = progressIndex =>
                    {
                        _progressIndex = progressIndex;
                        double percent = (double)_progressIndex / (double)_totalProgress * ConvertEndProgress;
                        int progress = Convert.ToInt32(percent);

                        _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                        {
                            _convertParam.experiencePage.Progress.Value = progress;
                        }));

                        return true;
                    };

                    var worker = new CombinePDFWorker(this, _srcFileList, _destinationDir, _originalFileList, progressFunc);
                    worker.Parent = _parent;
                    worker.ProgressIndex = _progressIndex;
                    worker.RemoveOriginFiles = convertOptPageDataContext.DeleteOriginalFiles;
                    worker.UseWipe = convertOptPageDataContext.IsUseWipe;
                    worker.NewPdfName = convertOptPageDataContext.NewPdfName;

                    worker.Transform();
                    GetSkipConvertButZipedFiles(worker);
                }
            }
            catch (Exception e)
            {
                _convertParam.experiencePage.Progress.Dispatcher.Invoke(new Action(delegate
                {
                    SimpleMessageWindows.DisplayWarningConfirmationMessage(e.Message);
                }));
            }
        }

        private void InitMetadataOptions()
        {
            if (_metadataOptions.Count > 0)
            {
                return;
            }

            var metaDataDic = new List<KeyValuePair<int, int>>();
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.ARTIST, (int)EXIFENUM.JEI_Artist));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.AUTHORS, (int)EXIFENUM.JEI_Artist));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.AUTHORS, (int)XMPDCENUM.Creators));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.AUTHORS, (int)DOCUMENTENUM.Author));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.AUTHORS_POSITION, (int)XMPPSENUM.AuthorsPosition));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.BASEURL, (int)XMPXBENUM.BaseUrl));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CAMERA_MAKE, (int)EXIFENUM.JEI_Make));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CAMERA_MODEL, (int)EXIFENUM.JEI_Model));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CAMERA_OWNER_NAME, (int)EXIFENUM.CameraOwnerName));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CAMERA_SERIAL_NUMBER, (int)EXIFENUM.BodySerialNumber));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CAPTION_WRITER, (int)XMPPSENUM.CaptionWriter));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CATEGORY, (int)DOCUMENTENUM.Category));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CITY, (int)XMPPSENUM.City));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.COMMENTS, (int)EXIFENUM.UserComment));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.COMMENTS, (int)DOCUMENTENUM.Comments));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.COMPANY_NAME, (int)DOCUMENTENUM.Company));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CONTRIBUTORS, (int)XMPDCENUM.Contributors));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.COUNTRY, (int)XMPPSENUM.Country));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CREATED_DATE, (int)EXIFENUM.JEI_DateTime));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CREATED_DATE, (int)XMPPSENUM.DateCreated));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CREATED_DATE, (int)XMPXBENUM.CreateDate));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CREATED_DATE, (int)DOCUMENTENUM.CreatedDate));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CREDIT, (int)XMPPSENUM.Credit));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.DESCRIPTION, (int)EXIFENUM.JEI_ImageDescription));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.GPS_LOCATION, (int)EXIFENUM.GPSData));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.HEADLINE, (int)XMPPSENUM.Headline));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.HISTORY, (int)XMPPSENUM.History));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.HYPERLINK, (int)DOCUMENTENUM.HyperlinkBase));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.INSTRUCTIONS, (int)XMPPSENUM.Instructions));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.KEYWORDS, (int)XMPPDFENUM.Keywords));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.KEYWORDS, (int)DOCUMENTENUM.Keywords));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.LABEL, (int)XMPXBENUM.Label));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.MANAGER_NAME, (int)DOCUMENTENUM.Manager));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.METADATA_DATE, (int)XMPXBENUM.MetadataDate));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.MODIFIED_DATE, (int)XMPXBENUM.ModifyDate));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.MODIFIED_DATE, (int)DOCUMENTENUM.ModifiedDate));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.NICKNAME, (int)XMPXBENUM.Nickname));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.PRODUCER, (int)XMPPDFENUM.Producer));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.PROGRAM_NAME, (int)EXIFENUM.JEI_Software));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.PROGRAM_NAME, (int)XMPXBENUM.CreatorTool));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.SOURCE, (int)XMPDCENUM.Source));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.SOURCE, (int)XMPPSENUM.Source));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.STATE, (int)XMPPSENUM.State));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.SUBJECT, (int)XMPDCENUM.Subject));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.SUBJECT, (int)DOCUMENTENUM.Subject));

            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.TITLE, (int)DOCUMENTENUM.Title));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.LASTSAVEDBY, (int)DOCUMENTENUM.LastSavedBy));
            metaDataDic.Add(new KeyValuePair<int, int>((int)METADATA_DISPLAY_NAME.CONTENTSTATUS, (int)DOCUMENTENUM.ContentStatus));

            var metadataOptList = new List<int>();

            for (int i = 0; i < (int)MedataNum.METADATA_COUNT; i++)
            {
                metadataOptList.Add((int)MedataNum.METADATA_START_NUM + i);
            }

            foreach (var key in metadataOptList)
            {
                foreach (var data in metaDataDic)
                {
                    if (data.Key == key)
                    {
                        _metadataOptions.Add(data.Value);
                    }
                }
            }
        }

        private void GetSkipConvertButZipedFiles(BaseConvertWorker worker)
        {
            foreach (var file in worker.SkipConvertButZipFileList)
            {
                if (!_skipConvertButZipFileList.Contains(file))
                {
                    _skipConvertButZipFileList.Add(file);
                }
            }
        }
    }
}