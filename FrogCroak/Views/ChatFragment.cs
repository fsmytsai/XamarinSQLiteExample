using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using FrogCroak.MyMethod;
using FrogCroakCL.Models;
using Java.IO;
using Android.Graphics;
using Java.Lang;
using Android.Content.PM;
using FrogCroakCL.Services;
using System.Threading.Tasks;
using System.Net;

namespace FrogCroak.Views
{
    public class ChatFragment : Fragment
    {
        private RecyclerView rv_MessageList;
        private EditText et_Message;
        private MessageListAdapter messageListAdapter;

        private MainActivity mainActivity;
        private List<ChatMessage> messageList;

        private bool isFirstLoad = true;
        private bool isLoading = true;
        private bool isFinishLoad = false;

        private const int REQUEST_EXTERNAL_STORAGE = 18;
        private FileChooser fileChooser;
        private ChatMessageService chatMessageService;
        private int nowPage = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.ChatFragment, container, false);
            mainActivity = (MainActivity)Activity;
            chatMessageService = new ChatMessageService();
            messageList = new List<ChatMessage>();
            initView(view);

            SetCache();

            ReadData();
            return view;
        }

        private void initView(View view)
        {
            rv_MessageList = (RecyclerView)view.FindViewById(Resource.Id.rv_MessageList);
            et_Message = (EditText)view.FindViewById(Resource.Id.et_Message);

            ImageButton ib_SendMessage = (ImageButton)view.FindViewById(Resource.Id.ib_SendMessage);
            ib_SendMessage.Click += delegate
            {
                SendMessage();
            };

            ImageButton ib_SendImage = (ImageButton)view.FindViewById(Resource.Id.ib_SendImage);
            ib_SendImage.Click += delegate
            {
                SendImage();
            };
        }

        MyLruCache lruCache;

        public void SetCache()
        {
            var maxMemory = (int)(Runtime.GetRuntime().MaxMemory() / 5);
            lruCache = new MyLruCache(maxMemory);
        }

        private void ReadData()
        {
            List<ChatMessage> ChatMessageList = chatMessageService.GetChatMessageList(nowPage);
            nowPage++;
            messageList.AddRange(ChatMessageList);
            if (ChatMessageList.Count < 15)
                isFinishLoad = true;

            if (isFirstLoad)
            {
                isFirstLoad = false;
                rv_MessageList.SetLayoutManager(new LinearLayoutManager(mainActivity, LinearLayoutManager.Vertical, true));
                messageListAdapter = new MessageListAdapter(this);
                rv_MessageList.SetAdapter(messageListAdapter);
                isLoading = false;
            }
            else
            {
                rv_MessageList.Post(() =>
                {
                    messageListAdapter.NotifyDataSetChanged();
                    isLoading = false;
                });
            }
        }

        public class MessageListAdapter : RecyclerView.Adapter
        {
            private ChatFragment chatFragment;

            private readonly int left = 87;
            private readonly int right = 78;

            public MessageListAdapter(ChatFragment chatFragment)
            {
                this.chatFragment = chatFragment;
            }

            public override int GetItemViewType(int position)
            {
                if (chatFragment.messageList[position].Isme)
                    return right;
                else
                    return left;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                Context context = parent.Context;
                View view;
                if (viewType == right)
                {
                    view = LayoutInflater.From(context).Inflate(Resource.Layout.RMessageBlock, parent, false);
                }
                else
                {
                    view = LayoutInflater.From(context).Inflate(Resource.Layout.LMessageBlock, parent, false);
                }
                ViewHolder viewHolder = new ViewHolder(view);
                return viewHolder;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ViewHolder vh = holder as ViewHolder;

                if (chatFragment.messageList[position].Type == 0)
                {
                    vh.tv_Message.Visibility = ViewStates.Visible;
                    if (GetItemViewType(position) == right)
                        vh.iv_Frog.Visibility = ViewStates.Gone;
                    vh.tv_Message.Text = chatFragment.messageList[position].Message;
                }
                else
                {
                    vh.tv_Message.Visibility = ViewStates.Gone;
                    vh.iv_Frog.Visibility = ViewStates.Visible;
                    if (chatFragment.lruCache.Get(chatFragment.messageList[position].Message) == null)
                    {
                        Bitmap bitmap = BitmapFactory.DecodeFile(chatFragment.mainActivity.FilesDir.AbsolutePath + "/" + chatFragment.messageList[position].Message);
                        chatFragment.lruCache.Put(chatFragment.messageList[position].Message, bitmap);
                        vh.iv_Frog.SetImageBitmap(bitmap);
                    }
                    else
                    {
                        Bitmap bitmap = (Bitmap)chatFragment.lruCache.Get(chatFragment.messageList[position].Message);
                        vh.iv_Frog.SetImageBitmap(bitmap);
                    }

                }

                //避免重複請求
                if (position > chatFragment.messageList.Count * 0.6 && !chatFragment.isFinishLoad && !chatFragment.isLoading)
                {
                    chatFragment.isLoading = true;
                    chatFragment.ReadData();
                }
            }

            public override int ItemCount => chatFragment.messageList.Count;

            public class ViewHolder : RecyclerView.ViewHolder
            {
                public TextView tv_Message { get; private set; }
                public ImageView iv_Frog { get; private set; }

                public ViewHolder(View itemView)
                        : base(itemView)
                {
                    tv_Message = (TextView)itemView.FindViewById(Resource.Id.tv_Message);
                    iv_Frog = (ImageView)itemView.FindViewById(Resource.Id.iv_Frog);
                }
            }
        }

        private async void SendMessage()
        {
            SharedService.HideKeyboard(mainActivity);
            mainActivity.activity_Outer.RequestFocus();

            string Message = et_Message.Text;

            if (Message.Trim() != "")
            {
                if (SharedService.CheckNetWork(mainActivity))
                {
                    et_Message.Text = "";
                    var chatMessage = new ChatMessage();
                    chatMessage.Message = Message;
                    chatMessage.Isme = true;
                    chatMessage.Type = 0;
                    bool IsSuccess = chatMessageService.CreateChatMessage(chatMessage);
                    if (IsSuccess)
                    {
                        messageList.Insert(0, chatMessage);
                        messageListAdapter.NotifyItemInserted(0);
                        rv_MessageList.ScrollToPosition(0);
                    }
                    else
                    {
                        SharedService.ShowTextToast("新增資料失敗", mainActivity);
                        return;
                    }
                }
                AllRequestResult result = null;
                await Task.Run(async () =>
                {
                    result = await chatMessageService.CreateMessage(Message);
                });
                if (result.IsSuccess)
                {
                    string ResMessage = (string)result.Result;
                    ResMessage = ResMessage.Replace("\\n", "\n");
                    var chatMessage = new ChatMessage();
                    chatMessage.Message = ResMessage;
                    chatMessage.Isme = false;
                    chatMessage.Type = 0;

                    bool IsSuccess = chatMessageService.CreateChatMessage(chatMessage);

                    if (IsSuccess)
                    {
                        messageList.Insert(0, chatMessage);
                        messageListAdapter.NotifyItemInserted(0);
                        rv_MessageList.ScrollToPosition(0);
                    }
                    else
                    {
                        SharedService.ShowTextToast("新增資料失敗", mainActivity);
                        return;
                    }
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, mainActivity);
                }
            }
            else
            {
                SharedService.ShowTextToast("請輸入內容", mainActivity);
            }
        }

        private void SendImage()
        {
            if (ActivityCompat.CheckSelfPermission(mainActivity, Android.Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted)
            {
                fileChooser = new FileChooser(mainActivity, this);
                if (!fileChooser.showFileChooser("image/*", null, false, true))
                {
                    SharedService.ShowTextToast("您沒有適合的檔案選取器", mainActivity);
                }
            }
            else
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.ReadExternalStorage }, REQUEST_EXTERNAL_STORAGE);
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
                        if (!fileChooser.showFileChooser("image/*", null, false, true))
                        {
                            SharedService.ShowTextToast("您沒有適合的檔案選取器", mainActivity);
                        }
                    }
                    else
                    {
                        SharedService.ShowTextToast("您拒絕選取檔案", mainActivity);
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
                    UploadImg(files);
                }
            }
        }

        private async void UploadImg(File[] files)
        {
            if (SharedService.CheckNetWork(mainActivity))
            {
                if (lruCache.Get(files[0].Name) == null)
                {
                    SharedService.ShowTextToast("壓縮圖片中...", mainActivity);
                    Bitmap bitmap = null;
                    await Task.Run(() =>
                    {
                        bitmap = SharedService.CompressImage(files[0].AbsolutePath, mainActivity.FilesDir.AbsolutePath + "/" + files[0].Name);
                    });

                    if (bitmap == null)
                    {
                        SharedService.ShowTextToast("壓縮圖片失敗QQ", mainActivity);
                        return;
                    }
                    lruCache.Put(files[0].Name, bitmap);
                }

                var chatMessage = new ChatMessage();
                chatMessage.Message = files[0].Name;
                chatMessage.Isme = true;
                chatMessage.Type = 1;

                bool IsSuccess = chatMessageService.CreateChatMessage(chatMessage);

                if (IsSuccess)
                {
                    messageList.Insert(0, chatMessage);
                    messageListAdapter.NotifyItemInserted(0);
                    rv_MessageList.ScrollToPosition(0);
                }
                else
                {
                    SharedService.ShowTextToast("新增資料失敗", mainActivity);
                    return;
                }

                SharedService.ShowTextToast("圖片上傳中...", mainActivity);
                string FileName = files[0].Name;
                string[] Type = FileName.Split('.');
                if (System.String.Compare(Type[Type.Length - 1], "jpg", true) == 0)
                    Type[Type.Length - 1] = "jpeg";

                AllRequestResult result = null;

                await Task.Run(async () =>
                {
                    result = await chatMessageService.UploadImage(mainActivity.FilesDir.AbsolutePath + "/" + files[0].Name, Type[Type.Length - 1]);
                });

                if (result.IsSuccess)
                {
                    chatMessage = new ChatMessage();
                    chatMessage.Message = (string)result.Result;
                    chatMessage.Isme = false;
                    chatMessage.Type = 0;

                    IsSuccess = chatMessageService.CreateChatMessage(chatMessage);

                    if (IsSuccess)
                    {
                        messageList.Insert(0, chatMessage);
                        messageListAdapter.NotifyItemInserted(0);
                        rv_MessageList.ScrollToPosition(0);
                    }
                    else
                    {
                        SharedService.ShowTextToast("新增資料失敗", mainActivity);
                        return;
                    }
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, mainActivity);
                }

            }
        }
    }
}