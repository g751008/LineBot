using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        /// <summary>
        /// Line機器人回覆API
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Post()
        {
            string MyLineChannelAccessToken = "znTqfeqd+8ZVKe7LxgSDAOf6zQH1MgujILXQP+sf2CsGhFgkC6XNth0w8yHyhPoswqsD2nLtnKeInRE3PKpyrXmXm3ZOBfBPgJdVmWjlElDkNzgk0k3q4Hr5TwXJB6qWhcLgyT4Cma84whA94GhZ5AdB04t89/1O/w1cDnyilFU=";
            string MyUserid = "U2a2428fbdcf1bcac5c044ae81a8ac994";

            try
            {
                //取得 http Post RawData(should be JSON)
                string postData = Request.Content.ReadAsStringAsync().Result;
                //剖析JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(postData);

                //建立LineBot物件實體
                isRock.LineBot.Bot LineBot = new isRock.LineBot.Bot(MyLineChannelAccessToken);

                if ("hi".Equals(ReceivedMessage.events[0].message.text.ToLower()) || (ReceivedMessage.events.FirstOrDefault().type == "follow"))
                {
                    var userInfo = LineBot.GetUserInfo(ReceivedMessage.events.FirstOrDefault().source.userId);
                    LineBot.ReplyMessage(ReceivedMessage.events.FirstOrDefault().replyToken, $"哈，'{userInfo.displayName}' 你來了...歡迎");
                }
                else if ("help".ToLower().Equals(ReceivedMessage.events[0].message.text.ToLower()))
                {
                    //發送圖片訊息
                    LineBot.PushMessage(MyUserid, "hi:打招呼\npic:秀圖片\nmenu:秀選單");
                }
                else if ("pic".ToLower().Equals(ReceivedMessage.events[0].message.text.ToLower()))
                {
                    //發送圖片訊息
                    LineBot.PushMessage(MyUserid,
                        new Uri("https://attach.setn.com/newsimages/2016/06/11/555280-XXL.jpg"));
                }
                else if ("menu".ToLower().Equals(ReceivedMessage.events[0].message.text.ToLower()))
                {
                    //建立actions，作為ButtonTemplate的用戶回覆行為
                    var actions = new List<isRock.LineBot.TemplateActionBase>();
                    actions.Add(new isRock.LineBot.MessageActon()
                    { label = "點選這邊等同用戶直接輸入某訊息", text = "/例如這樣" });
                    actions.Add(new isRock.LineBot.UriActon()
                    { label = "點這邊開啟網頁", uri = new Uri("http://www.google.com") });
                    actions.Add(new isRock.LineBot.PostbackActon()
                    { label = "點這邊發生postack", data = "abc=aaa&def=111" });

                    //單一Button Template Message
                    var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
                    {
                        altText = "替代文字(在無法顯示Button Template的時候顯示)",
                        text = "選單",
                        title = "選單測試",
                        //設定圖片
                        thumbnailImageUrl = new Uri("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1qk4H?ver=e9fb&q=90&m=6&h=450&w=800&l=f&f=jpg&o=t"),
                        actions = actions //設定回覆動作
                    };

                    //發送
                    LineBot.PushMessage(MyUserid, ButtonTemplate);
                }
                else
                {
                    //回覆訊息
                    string Message;
                    Message = string.Format("現在時間:{0}  您說了:{1}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"), ReceivedMessage.events[0].message.text);
                    //回覆用戶
                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, Message, MyLineChannelAccessToken);

                    Random R = new Random();
                    isRock.LineBot.Utility.PushStickerMessage(MyUserid, 1, R.Next(1, 17), MyLineChannelAccessToken);
                }

                //回覆API OK
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok();
            }
        }
    }
}
