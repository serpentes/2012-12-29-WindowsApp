using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// 這個檔案所定義的資料模型是做為強型別模型的代表範例，
// 以在加入、移除或修改成員時支援通知。選擇的屬性
// 名稱與標準項目範本中的資料繫結一致。
//
// 應用程式可以使用這個模型做為起點，再往上發展，也可以完全捨棄，
// 以適合需要的內容取代。

namespace App1.Data
{
    /// <summary>
    /// <see cref="SampleDataItem"/> 和 <see cref="SampleDataGroup"/> 的基底類別，
    /// 以定義兩者共通的屬性。
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : App1.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// 通用項目資料模型。
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// 通用群組資料模型。
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 提供完整項目集合的子集，以做為從 GroupedItemsPage 繫結的目標，
            // 原因有二: GridView 不會虛擬化大型項目集合，而且它
            // 在瀏覽有大量項目的群組時可以改進使用者
            // 經驗。
            //
            // 最多顯示 12 個項目，因為它會產生填滿的格線欄，
            // 不論顯示 1、2、3、4 或 6 列

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// 建立含硬式編碼內容的群組和項目集合。
    /// 
    /// SampleDataSource 是以預留位置資料初始化，而不是以即時生產環境
    /// 資料初始化，以便能夠於設計階段和執行階段提供範例資料。
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // 小型資料集可接受簡單線性搜尋
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // 小型資料集可接受簡單線性搜尋
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "?");
			

            var group1 = new SampleDataGroup("1",
                    "所有旅行的書",
                    "一覽，目前流浪等待浪漫的所有書籍。",
                    "Assets/MediumGray.png",
                    "no use :(");
            group1.Items.Add(new SampleDataItem("1-1",
                    "書名",
                    "作者/書的資料",
                    "Assets/LightGray.png",
                    "多行簡介",
                    "超多字的內文～～～在這裡～～～吼吼吼",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("2",
                    "我家書櫃",
                    "您的書想旅行嗎？",
                    "Assets/MediumGray.png",
                    "no use :(");
            group2.Items.Add(new SampleDataItem("2-1",
                    "我有新的書想旅行！",
                    "新增成員進書櫃",
                    "Assets/MediumGray.png",
                    "請輸入書名、作者、書況等各種資料",
                    "啦啦啦",
                    group2));
			group2.Items.Add(new SampleDataItem("2-2",
                    "樓下的房客",
                    "一行說明",
                    "Assets/MediumGray.png",
                    "多行說明",
                    "好多內文在這裡",
                    group2));

            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("3",
                    "旅行的初衷",
                    "旅行和夢想都是無遠弗屆的！",
                    "Assets/MediumGray.png",
                    "here no use :P");
            group3.Items.Add(new SampleDataItem("3-1",
                    "初衷 - 起點",
                    "旅行的最開始的理念",
                    "Assets/DarkGray.png",
                    "旅行，有一個起點，帶著期望，等待邂逅。",
					"在這個電子化的世代，你還理解書籍的本質嗎？\n再也不看的書是否已經逐漸淪為丟也不捨、留也無用的遺憾？\n\n讓你的夢想和書一同往世界的各個角落去吧！\n地球不如以往健康的環境，正需要我們重視森林和大氣的重要性，\n書本的重要性正因為電子化逐漸下滑，書不再需要被出版，\n這是我們體諒環境的方式，也是忘卻紙張觸感的遺憾，\n那些你再也不翻，卻又捨不得丟棄的書，請讓我們賦予他旅行的義務，\n\n帶著一份體諒在另外一個人手上復活。\n以交換的方式，重新珍惜書籍。\n\n\n由四個愛看書的女孩子共同研究開發，\n與其讓你的櫃子裡的書沾滿灰塵，不如就夾張漂亮的書籤，\n將這本書曾經帶給你的感動，旅行到下一個愛書人士的心理吧！\n\n\n",
                    group3));
            group3.Items.Add(new SampleDataItem("3-2",
                    "旅行的機票",
                    "如何讓你的書去旅行？幫他訂張機票吧！",
                    "Assets/DarkGray.png",
                    "旅行的方式有很多種，請讓我為您敘述其中奧妙！",
                    "【行前說明會】\n我們訂作這個個平台，\n讓你可以更新自己擁有的書籍、\n搜尋他人擁有的書籍\n或搜尋所在地附近有何種書籍，\n\n並且附上書評和推薦度，\n讓不常看書的你，可以透過大眾的評價謹慎選擇適合自己的讀物，\n而書籍狂熱者的你，可以為你所愛的類別撰寫，邂逅身處他方的同好。\n\n\n【注意事項】\n這是一個基於交流、捐獻二手書而成立的平台！\n\n01. 以捐贈為主。可自由做協議，需取得共識。\n02. 請對自己的二手書負責！將摺痕和汙損確實寫明！\n03. 約定交換請不要隨意變更，本系統設有黑名單。\n04.\n05.\n06.\n\n\n",
                    group3));
            group3.Items.Add(new SampleDataItem("3-3",
                    "以自由為主的行程表",
                    "旅行即將開始，你準備好了嗎？",
                    "Assets/DarkGray.png",
                    "讓我一一為你說明！",
                    "【我家書櫃】\n由此處新增你擁有的書籍，你家的書出去旅行的各個停駐點，都會更新在此處。\n\n\n【所有旅行的書】\n平台上所有的書籍都在這裡，\n誰更新了書籍？目前有多少書？\n通通都會在這裡即時顯示，\n你可以從這裡挑選喜歡的書目，進而去詢問這本書目前旅行到哪裡？\n\n\n【尋找旅行中的書！】\n在外旅行，突然有點想看看附近的書，\n這邊可以幫你搜尋喔！\n\n\n",
                    group3));
			group3.Items.Add(new SampleDataItem("3-4",
                    "個人資料設定",
                    "旅行是隨興的，但希望各位行事順利！",
                    "Assets/DarkGray.png",
                    "為了讓你的書旅行順利，這邊需要部分聯繫資料唷",
                    "【暱稱】\n【信箱】\n【個人書庫位置】\n\n\n",
                    group3));
			this.AllGroups.Add(group3);

            var group4 = new SampleDataGroup("4",
                    "尋找旅行中的書！",
                    "究竟有多少書等著你的邂逅呢？:P",
                    "Assets/MediumGray.png",
                    "here no use :P");
            group4.Items.Add(new SampleDataItem("4-1",
                    "這裡有旅行書嗎？",
                    "看看身邊，有多少書等著呢！",
                    "Assets/DarkGray.png",
                    "用地圖定位，搜尋附近有哪些書籍在等待邂逅呢？",
                    "等待更新\n\n",
                    group4));
			group4.Items.Add(new SampleDataItem("4-2",
                    "問題回報",
                    "您好，有以下問題這邊可以回報！",
                    "Assets/LightGray.png",
                    "我們將盡快為你處理:)",
                    "【有以下狀況請回報給我們，讓系統改善的更加好用:D】\n01. 無法遵守旅行約定者。\n02. 書籍多處損壞無據實以報。\n03.出言不遜、態度惡劣者。\n04.資料錯誤\n05.系統問題\n06.\n\n\n",
                    group4));
			
            this.AllGroups.Add(group4);
			
        }
    }
}
