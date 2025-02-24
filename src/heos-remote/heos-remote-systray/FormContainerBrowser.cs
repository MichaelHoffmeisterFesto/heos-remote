using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using heos_remote_lib;

namespace heos_remote_systray
{
    public partial class FormContainerBrowser : Form
    {
        //
        // public stuff
        //

        public HeosConnectedItem? Device = null;

        public List<HeosContainerLocation>? StartingPoints = null;

        public HeosContainerLocation CurrentLocation;

        public Func<Task<HeosSongQueue?>>? LambdaLoadSongQueue = null;

        public Func<HeosContainerLocation, HeosContainerItem?, int, Task>? LambdaPlayItem = null;

        public Func<Task>? LambdaClearQueue = null;

        public HeosContainerItem? ResultItem = null;

        public List<HeosContainerLocation> History = new();

        //
        // internal stuff
        //

        protected List<ContainerBrowserItem> _containerItems = new();

        protected BindingSource? _containersSource = null;

        protected HeosSongQueue? _queue = null;

        protected HeosImageCache _imgCache = new();

        public FormContainerBrowser()
        {
            InitializeComponent();
        }

        public async Task RefreshContainer(HeosContainerLocation loc)
        {
            // access
            if (Device == null)
                return;

            // display name in top
            if (loc.Name?.HasContent() == true)
                this.Text = "Browse: " + loc.Name;
            else
                this.Text = "Browse (unknown location)";

            // get items
            _containerItems = (await new HeosContainerList().Acquire(Device, loc.Sid, loc.Cid) ?? new HeosContainerList())
                .Select((hci) => new ContainerBrowserItem(hci)).ToList();

            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.DataContext = null;

            _containersSource = new BindingSource();

            _containersSource.DataSource = _containerItems;

            dataGridView1.DataSource = _containersSource;

            // test
            var indices = Enumerable.Range(0, _containerItems.Count);
            var client = new HttpClient();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 1 };

            await Parallel.ForEachAsync(indices, options, async (ndx, token) =>
            {
                ContainerBrowserItem? cbi = null;
                lock (_containerItems)
                {
                    if (ndx < 0 || ndx >= _containerItems.Count)
                        return;
                    cbi = (_containersSource[ndx] as ContainerBrowserItem)?.Copy();
                    if (cbi.DisplayImageUrl?.HasContent() != true)
                        return;
                }

                var response = await client.GetAsync(cbi.DisplayImageUrl);
                if (!response.IsSuccessStatusCode)
                    return;

                var ba = await response.Content.ReadAsByteArrayAsync();

                lock (_containerItems)
                {
                    if (ndx < 0 || ndx >= _containerItems.Count)
                        return;

                    cbi.DisplayImage = WinFormsUtils.ByteToImage(ba);
                    _containersSource[ndx] = cbi;
                }
            });
        }

        private async Task RefreshQueue()
        {
            // get
            if (LambdaLoadSongQueue != null)
                _queue = await LambdaLoadSongQueue.Invoke();

            // any
            if (_queue == null || _queue.Count < 1)
            {
                labelQueueCount.Text = "--";
                labelQueueNext.Text = "--";
                labelQueueLast.Text = "--";
            }
            else
            {
                var currNdx = _queue.SearchQidIndex(_queue.CurrentQid);

                if (currNdx >= 0)
                    labelQueueCount.Text = $"{1 + currNdx} of {_queue.Count}";
                else
                    labelQueueCount.Text = "" + _queue.Count;

                if (currNdx < _queue.Count - 1)
                    labelQueueNext.Text = "" + _queue[currNdx + 1]?.GetDisplayInfo();
                else
                    labelQueueNext.Text = "(end reached)";

                labelQueueLast.Text = "" + _queue.LastOrDefault()?.GetDisplayInfo();
            }
        }

        protected bool _initialFormLoad = false;

        private async void FormContainerBrowser_Load(object sender, EventArgs e)
        {
            // stupid
            _initialFormLoad = true;

            // action
            comboBoxAction.SelectedIndex = 0;

            // populate starting points
            if (StartingPoints != null)
            {
                foreach (var sp in StartingPoints)
                    comboBoxStarting.Items.Add("" + sp.Name);
            }

            // stupid 2
            _initialFormLoad = false;

            // updates
            await RefreshContainer(CurrentLocation);
            await RefreshQueue();
        }

        protected void ChargeRefreshQueue()
        {
            _ticksToRefreshQueue = 20;
        }

        protected int _ticksToRefreshQueue = 0;

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (_ticksToRefreshQueue > 0)
            {
                progressBarQueue.Value = _ticksToRefreshQueue;
                _ticksToRefreshQueue--;
                if (_ticksToRefreshQueue <= 0)
                {
                    _ticksToRefreshQueue = 0;
                    progressBarQueue.Value = 0;
                    await RefreshQueue();
                }
            }
        }

        private async Task RowDoubleClick(int rowIndex)
        {
            // access
            if (_containersSource == null)
                return;
            if (rowIndex < 0 || rowIndex >= _containersSource.Count)
                return;
            var cbi = _containersSource[rowIndex] as ContainerBrowserItem;
            if (cbi?.Item == null)
                return;

            // what to do?
            if (cbi.Item.Type == "heos_server" || cbi.Item.Type == "heos_service")
            {
                // top level list for this source
                CurrentLocation.Name = cbi.Item.Name;
                CurrentLocation.Sid = cbi.Item.Sid;
                CurrentLocation.Cid = "";
                await RefreshContainer(CurrentLocation);
            }
            else if (cbi.Item.IsContainer && cbi.Item.Cid?.HasContent() == true)
            {
                // container, go deeper
                History.Add(CurrentLocation.Copy());
                CurrentLocation.Name = CurrentLocation.Name + " / " + cbi.Item.Name;
                CurrentLocation.Cid = cbi.Item.Cid;
                await RefreshContainer(CurrentLocation);
            }
            else
            {
                // give immediate feedback
                progressBarQueue.Value = progressBarQueue.Maximum;

                // music
                if (LambdaPlayItem != null)
                    await LambdaPlayItem.Invoke(CurrentLocation, cbi.Item, 1 + comboBoxAction.SelectedIndex);

                // stay or leave
                if (comboBoxAction.SelectedIndex == 0 || comboBoxAction.SelectedIndex == 1)
                {
                    // leave, successful
                    progressBarQueue.Value = 0;
                    ResultItem = cbi.Item;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    // stay, eye candy
                    ChargeRefreshQueue();
                }
            }
        }

        private async void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // access
            await RowDoubleClick(e.RowIndex);
        }

        private async void button_Click(object sender, EventArgs e)
        {
            if (sender == buttonUp && History.Count > 0)
            {
                // pop
                var last = History.Last();
                History.RemoveAt(History.Count - 1);

                // refresh
                CurrentLocation = last.Copy();
                await RefreshContainer(CurrentLocation);
            }

            if (sender == buttonQueueClear && LambdaClearQueue != null)
            {
                await LambdaClearQueue.Invoke();
                ChargeRefreshQueue();
            }

            if (sender == buttonPlayAll && _containerItems.Count > 0
                && CurrentLocation.Cid?.HasContent() == true)
            {
                // give immediate feedback
                progressBarQueue.Value = progressBarQueue.Maximum;

                // item == null means the whole container
                if (LambdaPlayItem != null)
                    await LambdaPlayItem.Invoke(CurrentLocation, null, 1 + comboBoxAction.SelectedIndex);

                // stay or leave
                if (comboBoxAction.SelectedIndex == 0 || comboBoxAction.SelectedIndex == 1)
                {
                    // leave, successful
                    progressBarQueue.Value = 0;
                    ResultItem = null;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    // stay, eye candy
                    ChargeRefreshQueue();
                }
            }
        }

        private async void comboBoxStarting_SelectedIndexChanged(object sender, EventArgs e)
        {
            // access
            if (_initialFormLoad)
                return;
            var si = comboBoxStarting.SelectedIndex;
            if (StartingPoints == null || si < 0 || si >= StartingPoints.Count)
                return;

            // do it
            History.Add(CurrentLocation.Copy());
            CurrentLocation = StartingPoints[si].Copy();
            await RefreshContainer(CurrentLocation);
        }

        private void control_DoubleClick(object sender, EventArgs e)
        {
            if (sender == labelQueueCount)
            {
                ChargeRefreshQueue();
            }
        }
    }

    public class ContainerBrowserItem
    {
        public HeosContainerItem Item;

        public ContainerBrowserItem(HeosContainerItem item) { this.Item = item; }

        public string DisplayName { get {  return Item.Name; } }
        public string DisplayImageUrl { get { return Item.ImageUrl; } }
        
        public Bitmap? DisplayImage { get; set; } = null;

        public string DisplayLink
        {
            get
            {
                if (Item.IsContainer)
                    return ">";
                else if (Item.Type.Equals("station", StringComparison.CurrentCultureIgnoreCase))
                    return "\U0001F4E3";
                else
                    return "\u266b";  
            }
        }

        public ContainerBrowserItem Copy()
        {
            return new ContainerBrowserItem(Item)
            {
                DisplayImage = DisplayImage
            };
        }
    }
}
