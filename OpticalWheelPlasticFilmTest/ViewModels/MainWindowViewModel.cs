using HalconDotNet;
using Newtonsoft.Json;
using OpticalWheelPlasticFilmTest.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ViewROI;

namespace OpticalWheelPlasticFilmTest.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region 变量
        SXJ.Camera camera = new SXJ.Camera();
        // Create CancellationTokenSource.
        CancellationTokenSource source;
        // ... Get Token from source.
        CancellationToken token;
        bool cameraFree = true;
        SystemParam systemParam;
        #endregion
        #region 属性绑定
        private string _title = "OpticalWheelPlasticFilmTest";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string version = "1.0.0";
        public string Version
        {
            get { return version; }
            set { SetProperty(ref version, value); }
        }
        private string messageStr = "";
        public string MessageStr
        {
            get { return messageStr; }
            set { SetProperty(ref messageStr, value); }
        }
        private string curCamName;
        public string CurCamName
        {
            get { return curCamName; }
            set { SetProperty(ref curCamName, value); }
        }
        private ObservableCollection<string> cameraNameList = new ObservableCollection<string>();
        public ObservableCollection<string> CameraNameList
        {
            get { return cameraNameList; }
            set { SetProperty(ref cameraNameList, value); }
        }
        private string cameraNameListSelected;
        public string CameraNameListSelected
        {
            get { return cameraNameListSelected; }
            set { SetProperty(ref cameraNameListSelected, value); }
        }
        #region halcon
        private HImage _cameraIamge0;
        public HImage CameraIamge0
        {
            get { return _cameraIamge0; }
            set { SetProperty(ref _cameraIamge0, value); }
        }
        private bool _cameraRepaint0;
        public bool CameraRepaint0
        {
            get { return _cameraRepaint0; }
            set { SetProperty(ref _cameraRepaint0, value); }
        }
        private ObservableCollection<ROI> _cameraROIList0 = new ObservableCollection<ROI>();
        public ObservableCollection<ROI> CameraROIList0
        {
            get { return _cameraROIList0; }
            set { SetProperty(ref _cameraROIList0, value); }
        }
        private HObject _cameraAppendHObject0;
        public HObject CameraAppendHObject0
        {
            get { return _cameraAppendHObject0; }
            set { SetProperty(ref _cameraAppendHObject0, value); }
        }
        private HMsgEntry _cameraAppendHMessage0;
        public HMsgEntry CameraAppendHMessage0
        {
            get { return _cameraAppendHMessage0; }
            set { SetProperty(ref _cameraAppendHMessage0, value); }
        }
        private Tuple<string, object> _cameraGCStyle0;
        public Tuple<string, object> CameraGCStyle0
        {
            get { return _cameraGCStyle0; }
            set { SetProperty(ref _cameraGCStyle0, value); }
        }
        #endregion
        #endregion
        #region 方法绑定
        private DelegateCommand<object> menuCommand;
        public DelegateCommand<object> MenuCommand =>
            menuCommand ?? (menuCommand = new DelegateCommand<object>(ExecuteMenuCommand));
        private DelegateCommand appLoadedEventCommand;
        public DelegateCommand AppLoadedEventCommand =>
            appLoadedEventCommand ?? (appLoadedEventCommand = new DelegateCommand(ExecuteAppLoadedEventCommand));
        private DelegateCommand appClosedEventCommand;
        public DelegateCommand AppClosedEventCommand =>
            appClosedEventCommand ?? (appClosedEventCommand = new DelegateCommand(ExecuteAppClosedEventCommand));
        private DelegateCommand dropDownClosedEventCommand;

        public DelegateCommand DropDownClosedEventCommand =>
            dropDownClosedEventCommand ?? (dropDownClosedEventCommand = new DelegateCommand(ExecuteDropDownClosedEventCommand));

  
        #endregion
        #region 方法绑定函数
        void ExecuteMenuCommand(object obj)
        {
            switch (obj.ToString())
            {
                case "0":
                    break;
                case "6":
                    {
                        DialogParameters param = new DialogParameters();
                        param.Add("Title", Title);
                        param.Add("Version", Version);
                        dialogService.ShowDialog("AboutDialog", param, arg => { });
                    }
                    break;
                default:
                    break;
            }
        }
        void ExecuteAppClosedEventCommand()
        {
            try
            {
                camera.CloseCamera();
            }
            catch { }
        }
        void ExecuteAppLoadedEventCommand()
        {
            addMessage("软件加载完成");
            string[] directShowDevicesNames = camera.GetDevies("DirectShow");
            if (directShowDevicesNames != null)
            {
                foreach (string item in directShowDevicesNames)
                {
                    CameraNameList.Add(item);
                }
                //Json序列化，从文件读取
                string jsonString = File.ReadAllText(System.IO.Path.Combine(System.Environment.CurrentDirectory, "systemParam.json"));
                systemParam = JsonConvert.DeserializeObject<SystemParam>(jsonString);
                if (CameraNameList.Contains(systemParam.CurCamName))
                {
                    CameraNameListSelected = CurCamName = systemParam.CurCamName;
                    source = new CancellationTokenSource(); 
                    token = source.Token;
                    cameraFree = false;
                    Task.Run(() => grabImageAciton(token, CameraNameListSelected), token).ContinueWith(t => cameraFree = true);
                    addMessage($"开启{CameraNameListSelected}相机");
                }                
            }
            else
            {
                System.Windows.MessageBox.Show("未找到\"USB相机\"设备");
                System.Environment.Exit(-1);
            }
        }
        void ExecuteDropDownClosedEventCommand()
        {
            if (CameraNameListSelected != CurCamName)
            {
                if (source != null)
                {
                    source.Cancel();
                }
                while (!cameraFree)
                {
                    System.Threading.Thread.Sleep(100);
                }
                source = new CancellationTokenSource();
                token = source.Token;
                cameraFree = false;
                Task.Run(() => grabImageAciton(token, CameraNameListSelected), token).ContinueWith(t => cameraFree = true);
                CurCamName = CameraNameListSelected;
                systemParam.CurCamName = CameraNameListSelected;
                string jsonString = JsonConvert.SerializeObject(systemParam, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(System.IO.Path.Combine(System.Environment.CurrentDirectory, "systemParam.json"), jsonString);
                addMessage($"切换到{CameraNameListSelected}相机");
            }
        }
        #endregion
        #region 构造函数

        private readonly IDialogService dialogService;
        public MainWindowViewModel(IDialogService _dialogService)
        {
            dialogService = _dialogService;
        }
        #region 自定义函数
        private void addMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        private void grabImageAciton(CancellationToken token,string cameraName)
        {
            camera.OpenCamera(cameraName, "DirectShow");
            camera.GrabeImageStart();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    camera.CloseCamera();
                    return;
                }
                CameraIamge0 = camera.GrabeImageAsync();
                //System.Threading.Thread.Sleep(100);
            }
        }
        #endregion
        #region 运行
        
        #endregion
        #endregion

    }
}
