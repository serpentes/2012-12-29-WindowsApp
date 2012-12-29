using App1.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 分割頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234234

namespace App1
{
    /// <summary>
    /// 顯示群組標題、群組內之項目清單和
    /// 目前選取項目之詳細資料的頁面。
    /// </summary>
    public sealed partial class SplitPage : App1.Common.LayoutAwarePage
    {
        public SplitPage()
        {
            this.InitializeComponent();
        }

        #region 頁面狀態管理

        /// <summary>
        /// 巡覽期間以傳遞的內容填入頁面。從之前的工作階段
        /// 重新建立頁面時，也會提供儲存的狀態。
        /// </summary>
        /// <param name="navigationParameter">最初要求這個頁面時，傳遞到
        /// <see cref="Frame.Navigate(Type, Object)"/> 的參數。
        /// </param>
        /// <param name="pageState">這個頁面在先前的工作階段期間保留的
        /// 狀態字典。第一次瀏覽頁面時，這一項是 null。</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: 為您的問題領域建立適合的資料模型，以取代資料範例
            var group = SampleDataSource.GetGroup((String)navigationParameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;

            if (pageState == null)
            {
                this.itemListView.SelectedItem = null;
                // 當這是新頁面時，自動選取第一個項目，除非正在使用
                // 邏輯頁面巡覽 (參閱底下的邏輯頁面巡覽 #region)。
                if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null)
                {
                    this.itemsViewSource.View.MoveCurrentToFirst();
                }
            }
            else
            {
                // 還原之前儲存、與這個頁面關聯的狀態
                if (pageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
                {
                    var selectedItem = SampleDataSource.GetItem((String)pageState["SelectedItem"]);
                    this.itemsViewSource.View.MoveCurrentTo(selectedItem);
                }
            }
        }

        /// <summary>
        /// 在應用程式暫停或從巡覽快取中捨棄頁面時，
        /// 保留與這個頁面關聯的狀態。值必須符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化需求。
        /// </summary>
        /// <param name="pageState">即將以可序列化狀態填入的空白字典。</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (this.itemsViewSource.View != null)
            {
                var selectedItem = (SampleDataItem)this.itemsViewSource.View.CurrentItem;
                if (selectedItem != null) pageState["SelectedItem"] = selectedItem.UniqueId;
            }
        }

        #endregion

        #region 邏輯頁面巡覽

        // 視覺狀態管理通常會直接反映四個應用程式檢視狀態
        // (全螢幕橫向及縱向加上快照檢視和填滿檢視)。分割頁面
        // 的設計可讓快照檢視狀態和縱向檢視狀態有兩個不同的子狀態:
        // 可能會顯示項目清單或詳細資料，但不能同時顯示兩者。
        //
        // 這全部都是使用可以代表兩個邏輯頁面的單一實體頁面實作。
        // 以下程式碼可以在不讓使用者感知差異的情況下達成此
        // 目標。

        /// <summary>
        /// 叫用以判斷頁面是否應該當做一個邏輯頁面或兩個邏輯頁面。
        /// </summary>
        /// <param name="viewState">提出之問題的檢視狀態或目前檢視狀態之 null。
        /// 使用 null 做為預設值時，這個參數是選擇性的
        /// 。</param>
        /// <returns>當問題中的檢視狀態是縱向或快照時為 True，否則為 false
        /// 。</returns>
        private bool UsingLogicalPageNavigation(ApplicationViewState? viewState = null)
        {
            if (viewState == null) viewState = ApplicationView.Value;
            return viewState == ApplicationViewState.FullScreenPortrait ||
                viewState == ApplicationViewState.Snapped;
        }

        /// <summary>
        /// 在選取清單內之項目時叫用。
        /// </summary>
        /// <param name="sender">GridView (或快照應用程式時為 ListView)
        /// 顯示選取的項目。</param>
        /// <param name="e">描述選取範圍如何變更的事件資料。</param>
        void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 在邏輯頁面巡覽生效時使檢視狀態失效，因為選取範圍內的變更
            // 可能在目前邏輯頁面中造成對應的變更。當
            // 已選取項目時，這會有從顯示項目清單變更為顯示
            // 選取之項目的詳細資料的效果。當選取範圍已清除時，這會有
            // 相反的效果。
            if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
        }

        /// <summary>
        /// 按下頁面的上一頁按鈕時叫用。
        /// </summary>
        /// <param name="sender">上一頁按鈕執行個體。</param>
        /// <param name="e">描述按一下上一頁按鈕之方式的事件資料。</param>
        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (this.UsingLogicalPageNavigation() && itemListView.SelectedItem != null)
            {
                // 邏輯頁面巡覽生效時，則會有選取的項目，其就是
                // 目前顯示之項目詳細資料。清除選取範圍將返回
                // 項目清單。從使用者的觀點，這是邏輯向後
                // 巡覽。
                this.itemListView.SelectedItem = null;
            }
            else
            {
                // 邏輯頁面巡覽未生效時，或沒有選取的
                // 項目時，請使用預設的上一頁按鈕行為。
                base.GoBack(sender, e);
            }
        }

        /// <summary>
        /// 叫用以判斷對應到應用程式檢視狀態之視覺狀態的
        /// 檢視狀態。
        /// </summary>
        /// <param name="viewState">提出問題之檢視狀態。</param>
        /// <returns>想要之視覺狀態的名稱。這與檢視狀態的名稱相同，
        /// 但若在縱向及快照檢視中已有選取的項目，且
        /// 檢視中這個其他的邏輯頁面是以加入 _Detail 後置字元來表示時則不然。</returns>
        protected override string DetermineVisualState(ApplicationViewState viewState)
        {
            // 當檢視狀態變更時，更新上一頁按鈕的啟用狀態
            var logicalPageBack = this.UsingLogicalPageNavigation(viewState) && this.itemListView.SelectedItem != null;
            var physicalPageBack = this.Frame != null && this.Frame.CanGoBack;
            this.DefaultViewModel["CanGoBack"] = logicalPageBack || physicalPageBack;

            // 不根據檢視狀態決定橫向配置的視覺狀態，而是
            // 根據視窗的寬度。這一頁有一個配置用於
            // 1366 或更寬的虛擬像素，還有另一個配置用於較窄的顯示幕或用在次
            // 應用程式將水平可用空間縮減到小於 1366 時。
            if (viewState == ApplicationViewState.Filled ||
                viewState == ApplicationViewState.FullScreenLandscape)
            {
                var windowWidth = Window.Current.Bounds.Width;
                if (windowWidth >= 1366) return "FullScreenLandscapeOrWide";
                return "FilledOrNarrow";
            }

            // 直向或有次應用程式時，從預設視覺狀態名稱開始，然後在檢視
            // 詳細資料 (不是清單) 時加入尾碼
            var defaultStateName = base.DetermineVisualState(viewState);
            return logicalPageBack ? defaultStateName + "_Detail" : defaultStateName;
        }

        #endregion
    }
}
