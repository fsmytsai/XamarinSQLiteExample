using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using FrogCroak.MyMethod;
using Android.Content.PM;
using Java.IO;
using FrogCroakCL.Models;
using System.Threading.Tasks;
using FrogCroakCL.Services;
using System.Net;

namespace FrogCroak.Views
{
    public class SoundRecognitionFragment : Fragment
    {
        private FileChooser fileChooser;
        private const int REQUEST_EXTERNAL_STORAGE = 18;
        private const int REQUEST_RECORD_AUDIO = 22;
        private ImageView iv_AudioFrog;
        private ImageButton ib_RecordWav;
        private TextView tv_Result;
        private MainActivity mainActivity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnStop()
        {
            base.OnStop();
            if (AudioRecordFunc.isRecord)
            {
                AudioRecordFunc.getInstance(mainActivity).stopRecordAndFile();
                SharedService.ShowTextToast("錄音停止", mainActivity);
                ib_RecordWav.SetImageResource(Resource.Drawable.recordbtn);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.SoundRecognitionFragment, container, false);
            mainActivity = (MainActivity)Activity;
            view.FindViewById(Resource.Id.ib_UploadWav).Click += delegate
            {
                SendWav();
            };
            ib_RecordWav = view.FindViewById<ImageButton>(Resource.Id.ib_RecordWav);
            ib_RecordWav.Click += delegate
            {
                RecordWav();
            };
            iv_AudioFrog = (ImageView)view.FindViewById(Resource.Id.iv_AudioFrog);
            tv_Result = (TextView)view.FindViewById(Resource.Id.tv_Result);
            return view;
        }

        public void SendWav()
        {
            if (ActivityCompat.CheckSelfPermission(mainActivity, Android.Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted)
            {
                fileChooser = new FileChooser(mainActivity, this);
                if (!fileChooser.showFileChooser("audio/x-wav", null, false, true))
                {
                    SharedService.ShowTextToast("您沒有適合的檔案選取器", mainActivity);
                }
                else
                {
                    tv_Result.Text = "";
                    iv_AudioFrog.Visibility = ViewStates.Gone;
                }
            }
            else
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.ReadExternalStorage }, REQUEST_EXTERNAL_STORAGE);
            }
        }

        public void RecordWav()
        {
            if (ActivityCompat.CheckSelfPermission(mainActivity, Android.Manifest.Permission.RecordAudio) == (int)Permission.Granted)
            {
                AudioRecordFunc mRecord = AudioRecordFunc.getInstance(mainActivity);
                if (AudioRecordFunc.isRecord)
                {
                    mRecord.stopRecordAndFile();
                    SharedService.ShowTextToast("錄音停止", mainActivity);
                    UploadWavAsync(mRecord.NewAudioName);
                    ib_RecordWav.SetImageResource(Resource.Drawable.recordbtn);
                }
                else
                {
                    mRecord.startRecordAndFile();
                    SharedService.ShowTextToast("錄音開始，再次按下按鈕即結束錄音", mainActivity);

                    tv_Result.Text = "";
                    iv_AudioFrog.Visibility = ViewStates.Gone;
                    ib_RecordWav.SetImageResource(Resource.Drawable.recordingbtn);
                }
            }
            else
            {
                RequestPermissions(new string[] {
                    Android.Manifest.Permission.RecordAudio
                }, REQUEST_RECORD_AUDIO);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case REQUEST_EXTERNAL_STORAGE:
                    if (grantResults.Length > 0 && grantResults[0] == (int)Permission.Granted)
                    {
                        fileChooser = new FileChooser(mainActivity, this);
                        if (!fileChooser.showFileChooser("audio/x-wav", null, false, true))
                        {
                            SharedService.ShowTextToast("您沒有適合的檔案選取器", mainActivity);
                        }
                        else
                        {
                            tv_Result.Text = "";
                            iv_AudioFrog.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        SharedService.ShowTextToast("您拒絕選取檔案", mainActivity);
                    }
                    return;
                case REQUEST_RECORD_AUDIO:
                    if (grantResults.Length > 0 && grantResults[0] == (int)Permission.Granted)
                    {
                        AudioRecordFunc mRecord = AudioRecordFunc.getInstance(mainActivity);
                        mRecord.startRecordAndFile();
                        SharedService.ShowTextToast("錄音開始，再次按下按鈕即結束錄音", mainActivity);

                        tv_Result.Text = "";
                        iv_AudioFrog.Visibility = ViewStates.Gone;
                        ib_RecordWav.SetImageResource(Resource.Drawable.recordingbtn);
                    }
                    else
                    {
                        SharedService.ShowTextToast("您拒絕錄音", mainActivity);
                    }
                    return;
            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == FileChooser.ACTIVITY_FILE_CHOOSER)
            {
                if (fileChooser.onActivityResult(requestCode, resultCode, data))
                {
                    File[] files = fileChooser.getChosenFiles();
                    UploadWavAsync(files[0].AbsolutePath);
                }
            }
        }

        private async void UploadWavAsync(string AbsolutePath)
        {
            if (SharedService.CheckNetWork(mainActivity))
            {
                SharedService.ShowTextToast("辨識中...", mainActivity);
                AllRequestResult result = null;
                await Task.Run(async () =>
                {
                    result = await new SoundRecognitionService().SoundRecognition(AbsolutePath);
                });
                if (result.IsSuccess)
                {
                    string FrogName = (string)result.Result;
                    tv_Result.Text = FrogName;
                    iv_AudioFrog.Visibility = ViewStates.Visible;
                    if (FrogName == "台北樹蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.tptreefrog);
                    else if (FrogName == "面天樹蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.facefrog);
                    else if (FrogName == "澤蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.waterfrog);
                    else if (FrogName == "小雨蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.rainfrog);
                    else if (FrogName == "艾氏樹蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.aistreefrog);
                    else if (FrogName == "拉都希氏赤蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.ladofrog);
                    else if (FrogName == "虎皮蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.lionskinfrog);
                    else if (FrogName == "豎琴蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.harpfrog);
                    else if (FrogName == "布氏樹蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.boostreefrog);
                    else if (FrogName == "貢德氏赤蛙")
                        iv_AudioFrog.SetImageResource(Resource.Drawable.gondesfrog);
                    else
                        iv_AudioFrog.SetImageResource(Resource.Drawable.people);
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, mainActivity);
                }
            }
            else
            {
                SharedService.ShowTextToast("請檢察網路連線", mainActivity);
            }
        }
    }
}