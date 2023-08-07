using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Xml;
using ThreeL.Client.Win.Models;

namespace ThreeL.Client.Win.Helpers
{
    public class EmojiHelper
    {
        public Dictionary<string, BitmapImage> EmojiCode = new Dictionary<string, BitmapImage>();
        private List<EmojiEntity> emojiList = new List<EmojiEntity>();
        /// <summary>
        /// emoji集合
        /// </summary>
        public List<EmojiEntity> EmojiList
        {
            get
            {
                return emojiList;
            }

            set
            {
                emojiList = value;
            }
        }

        public EmojiHelper()
        {
            AnayXML();
        }

        /// <summary>
        /// 解析xml
        /// </summary>
        public void AnayXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream _stream = _assembly.GetManifestResourceStream("ThreeL.Client.Win.Emoji.xml");
            xmlDoc.Load(_stream);
            XmlNode root = xmlDoc.SelectSingleNode("array");
            XmlNodeList nodeList = root.ChildNodes;
            //循环列表，获得相应的内容
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;
                XmlNodeList subList = xe.ChildNodes;
                EmojiEntity entity = new EmojiEntity();
                foreach (XmlNode xmlNode in subList)
                {
                    if (xmlNode.Name == "key")
                    {
                        entity.Key = xmlNode.InnerText;
                    }
                    if (xmlNode.Name == "array")
                    {
                        XmlElement lastXe = (XmlElement)xmlNode;
                        foreach (XmlNode lastNode in lastXe)
                        {
                            if (lastNode.Name == "a")
                            {
                                entity.EmojiCode.Add(GetEmojiStr(lastNode.InnerText), GetEmojiImage(lastNode.Attributes[1].Value));
                            }
                        }
                    }
                }
                EmojiList.Add(entity);
            }
            foreach (var item in EmojiList)
            {
                //所有的内容都添加到一个dictionary中
                EmojiCode = EmojiCode.Concat(item.EmojiCode).ToDictionary(k => k.Key, v => v.Value);
            }
        }
        /// <summary>
        /// 返回Emoji字符串
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetEmojiStr(string name)
        {
            return "[" + name + "]";
        }
        /// <summary>
        /// 返回Emoji图像
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private BitmapImage GetEmojiImage(string name)
        {
            BitmapImage bitmap = new BitmapImage();
            string imgUrl = "pack://application:,,,/ThreeL.Client.Win;component/Images/" + name + ".png";
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            return bitmap;
        }
    }
}
