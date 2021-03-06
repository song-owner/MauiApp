using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
//test
namespace MauiApp1

{
    public partial class ResultPage : ContentPage
    {
        string colloredLineUrl = "";
        double sliderValue = 0.5;
        List<Problems> problemList = new List<Problems>();
        FirebaseHelper firebaseHelper = new FirebaseHelper();
        WebView webView = new WebView();
        static int listRow = 0;
        Problems lastProblems = new Problems();
        string lastItemTappedUrl = "";
        public ResultPage(string finedWord)
        {
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromSeconds(0.2), () =>
            {
                seartchWord.Text = finedWord;
                return false;
            });
            lastProblems.SearchWord = finedWord;
            webView.Source = "https://www.google.com/search?q=" + finedWord;
            webView.VerticalOptions = LayoutOptions.FillAndExpand;
            browser.Children.Add(webView);
            webView.Navigated += new EventHandler<WebNavigatedEventArgs>(OnNavigated);
            string uuid = "";
            //if (DependencyService.Get<IDeviceService>() != null)
            //{
            //    uuid = DependencyService.Get<IDeviceService>().GetUuid();
            //}
            lastProblems.PhoneId = uuid;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            refreshList(seartchWord.Text);
        }
        string distWord(int sel, Problems word)
        {
            int first = 0;
            int state = 0;
            int lastState = 0;
            string pack = "";
            string distWord = "";
            if (sel == 0) distWord = word.SearchWord;
            if (sel == 1 && word.HtmlTitle != null)
            {
                foreach (var abc in word.HtmlTitle)
                {
                    if (word.HtmlTitle.IndexOf("??????") >= 0)
                    { }
                    //????????????
                    if (('???' <= abc && abc <= '???') && !('???' <= abc && abc <= '???'))
                    { state = 0; }
                    //????????????
                    if ('???' <= abc && abc <= '???')
                    { state = 1; }
                    //??????
                    if ('???' <= abc && abc <= '???')
                    { state = 2; }
                    //?????????????????????
                    if (('a' <= abc && abc <= 'z') || ('A' <= abc && abc <= 'Z'))
                    { state = 3; }
                    if (first != 0 && state != lastState)
                        pack = pack + " ";
                    pack = pack + abc;
                    lastState = state;
                    first++;
                }
                distWord = pack;
            }
            return distWord;
        }
        async void refreshList(string seartchWord, int sel = 0)
        {
            var allProblems = await firebaseHelper.GetAllProblems();
            if (allProblems == null) return;
            var sameWord = new List<string>();
            var sameWordLast = new List<string>();
            if (seartchWord != "" && seartchWord != null && allProblems.Count > 0)
            {
                //?????????????????????????????????
                var problems = new List<string>();
                foreach (var tmp in allProblems)
                {
                    int splitCount = 0;
                    foreach (var dist in distWord(sel, tmp).Split())
                    {
                        //?????????????????????????????????
                        problems.Add(distWord(sel, tmp).Split()[splitCount]);
                        splitCount++;
                    }
                }
                problems = problems.Distinct().ToList();
                //?????????????????????????????????
                int splitNo = 1;
                int num = 0;
                foreach (string seartchs in seartchWord.Split())
                {
                    sameWord = seartchs.SortByDistance(problems).ToList();
                    sameWord = sameWord.Distinct().ToList();
                    //???????????????????????????
                    foreach (string result in sameWord)
                    {
                        //if (sameWordLast.Count <= num) sameWordLast.Add(result);
                        //else
                        sameWordLast.Insert(num, result);
                        num += splitNo;
                    }
                    num = splitNo;
                    splitNo++;
                    //sameWord.AddRange(seartchs.SortByDistance(problems).ToList());
                    //sameWord = sameWord.Distinct().ToList();
                }
                //sameWord = seartchWord.Text.SortByDistance(problems).ToList();
                //???????????????????????????????????????????????????????????????
                sameWordLast = sameWordLast.Distinct().ToList();
                int count = 0;
                problemList.Clear();
                //?????????????????????????????????
                foreach (var tmp in sameWordLast)
                {
                    if (tmp == "") continue;
                    var maching = new List<Problems>();
                    //???????????????????????????????????????????????????????????????
                    foreach (var allprob in allProblems)
                    {
                        //??????????????????????????????????????????????????????
                        bool letContinue = false;
                        foreach (Problems pro in problemList)
                            if (allprob.Url == pro.Url)
                                letContinue = true;
                        if (letContinue == true) continue;
                        //?????????????????????????????????
                        var allprbSplit = distWord(sel, allprob).Split();
                        foreach (var allprobSplited in allprbSplit)
                        {
                            if (allprobSplited == tmp)
                            {
                                maching.Add(allprob);
                            }
                        }
                    }
                    //var problems2 = allProblems.
                    //    Where(a => a.SearchWord != null && a.SearchWord.IndexOf(tmp) >= 0).ToList();
                    if (count == 0) problemList = maching; else problemList.InsertRange(problemList.Count, maching);
                    if (count == 7) break;
                    count++;
                }
            }
            else
            {
                problemList = allProblems;
            }
            foreach (var tmp in problemList)
            {
                if (tmp.PhoneId == lastProblems.PhoneId)
                {
                    tmp.Del = true;
                    tmp.DelWidth1 = new GridLength(0.2, GridUnitType.Star);
                    tmp.DelWidth2 = new GridLength(0.2, GridUnitType.Star);
                }
                else
                {
                    tmp.Del = false;
                    tmp.DelWidth1 = new GridLength(0.4, GridUnitType.Star);
                    tmp.DelWidth2 = new GridLength(0, GridUnitType.Star);
                }
            }
            //problemList.Sort((a, b) => int.Parse(b.Nice) - int.Parse(a.Nice));
            //foreach (var abc in problemList) { abc.ListColor = Colors.Red; }
            foreach (var abc in problemList)
                if (colloredLineUrl == abc.Url)
                    abc.ListColor = Colors.Yellow;
                else
                    abc.ListColor = Colors.WhiteSmoke;
            //problemListView.ItemsSource = null;
            problemListView.ItemsSource = problemList;
        }
        private async void itemTapped(object sender, ItemTappedEventArgs e)
        {
            listRow = e.ItemIndex;
            if (listRow < 0)
            {
                return;
            }
            var now = problemList[listRow];
            if (now.Url == null || now.Url.IndexOf("http") == -1)
            {
                return;
            }
            colloredLineUrl = now.Url;
            seartchWord.Text = now.SearchWord;
            refreshList(seartchWord.Text);
            webView.Source = now.Url;
            lastProblems.SearchWord = now.SearchWord;
            lastItemTappedUrl = now.Url;

            //lineCollorChange(now.Url);
        }
        private async void deleteTaped(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string key = button.CommandParameter.ToString();
            await firebaseHelper.DeleteProblem(key);
            refreshList(seartchWord.Text);
        }
        private async void startSearch(object sender, EventArgs e)
        {
            var url = "https://www.google.com/search?q=" + seartchWord.Text;
            if (seartchWord.Text != null && seartchWord.Text.IndexOf("http") >= 0) url = seartchWord.Text;
            webView.Source = url;
            refreshList(seartchWord.Text);
            lastProblems.SearchWord = seartchWord.Text;
        }
        private async void saveUrl(object sender, EventArgs e)
        {
            await solvedResult(true);
            colloredLineUrl = lastProblems.Url;
            //lineCollorChange(lastProblems.Url);
        }
        async Task<bool> solvedResult(bool result)
        {
            //if (result == false)
            //    result = await DisplayAlert("Did the search word \"" + lastProblems.SearchWord + "\" resolve on the last searched page?", "", "Yes", "No");
            if (result == true)
            {
                var title = await AsyncWork2(lastProblems.Url);
                lastProblems.HtmlTitle = title;

                var sameUrl = await firebaseHelper.GetProblem(lastProblems.Url);
                if (sameUrl == null)
                {
                    lastProblems.Nice = "1";
                    await firebaseHelper.AddProblem(
                        lastProblems.PhoneId, lastProblems.SearchWord, lastProblems.HtmlTitle, lastProblems.Url, lastProblems.Nice);
                }
                else
                {
                    var nice = 0;
                    if (sameUrl.Nice != null) nice = Int32.Parse(sameUrl.Nice);
                    nice++;
                    await firebaseHelper.UpdatePerson(sameUrl.Key, lastProblems.PhoneId, lastProblems.SearchWord, lastProblems.HtmlTitle, lastProblems.Url, nice.ToString());
                }
            }
            refreshList(seartchWord.Text);
            lastProblems.SearchWord = seartchWord.Text;
            return true;
        }
        private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            var url = e.Url;
            //testUrl.Text = url;
            //var title = await AsyncWork2(url);
            //lastProblems.HtmlTitle = title;
            lastProblems.Url = url;
        }
        private void resultPageChange(object sender, PropertyChangedEventArgs e)
        {
            if (resultPage.Width < 0) return;
            slider.Value = sliderValue;
            AppPge.Width = resultPage.Width * sliderValue;
            GooglePage.Width = GridLength.Star;
        }
        static async Task<string> AsyncWork2(string urlstring)
        {
            var client = new HttpClient();
            var parser = new HtmlParser();
            var doc = default(IHtmlDocument);
            try
            {
                var stream = await client.GetStreamAsync(new Uri(urlstring));
                doc = await parser.ParseDocumentAsync(stream);
                var title = doc.GetElementsByTagName("title")[0].InnerHtml;
                return title;
            }
            catch { }
            return "";
        }
        private async void closeClick(object sender, EventArgs e)
        {
            //DependencyService.Get<IDeviceService>().Exit();
        }

        private void backClick(object sender, EventArgs e)
        {
            if (webView.CanGoBack) webView.GoBack();
        }

        void enterEvent(System.Object sender, System.EventArgs e)
        {
            object a = null;
            EventArgs b = null;
            startSearch(a, b);
        }

        void serchSubEvent(System.Object sender, System.EventArgs e)
        {
            refreshList(serchSub.Text);
        }

        void titleSubEvent(System.Object sender, System.EventArgs e)
        {
            refreshList(titleSub.Text, 1);
        }
    }
}
