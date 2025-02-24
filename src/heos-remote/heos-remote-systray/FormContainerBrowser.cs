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

        public List<HeosContainerStartingPoint>? StartingPoints = null;

        private int _sid = 3;
        public int Sid { get { return _sid; } set { _sid = value; } }

        private string _cid = "";
        public string Cid { get { return _cid; } set { _cid = value; } }

        public Func<Task<HeosSongQueue?>>? LambdaLoadSongQueue = null;

        public Func<int, string, HeosContainerItem, int, Task>? LambdaPlayItem = null;

        public HeosContainerItem? ResultItem = null;

        public List<HeosContainerStartingPoint> History = new();

        //
        // internal stuff
        //

        protected List<ContainerBrowserItem> _containerItems = new();

        protected BindingSource? _containersSource = null;

        protected HeosSongQueue? _queue = null;

        public FormContainerBrowser()
        {
            InitializeComponent();            
        }

        public async Task RefreshContainer(int sid, string? cid)
        {
            // access
            if (Device == null)
                return;

            // get items
            _containerItems = (await new HeosContainerList().Acquire(Device, sid, cid) ?? new HeosContainerList())
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
                var cbi = (_containersSource[ndx] as ContainerBrowserItem)?.Copy();
                if (cbi.DisplayImageUrl?.HasContent() != true)
                    return;

                var response = await client.GetAsync(cbi.DisplayImageUrl);
                if (!response.IsSuccessStatusCode)
                    return;
                var ba = await response.Content.ReadAsByteArrayAsync();
                cbi.DisplayImage = WinFormsUtils.ByteToImage(ba);
                _containersSource[ndx] = cbi;
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
                labelQueueCount.Text = "" + _queue.Count;
                labelQueueNext.Text = "" + _queue.FirstOrDefault()?.GetDisplayInfo();
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
            await RefreshContainer(_sid, _cid);
            await RefreshQueue();
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
                _sid = cbi.Item.Sid;
                await RefreshContainer(sid: cbi.Item.Sid, cid: null);
            }
            else if (cbi.Item.IsContainer && cbi.Item.Cid?.HasContent() == true)
            {
                // container, go deeper
                History.Add(new HeosContainerStartingPoint() { Sid = _sid, Cid = _cid });
                _cid = cbi.Item.Cid;
                await RefreshContainer(_sid, _cid);
            }
            else
            {
                // give immediate feedback
                progressBarQueue.Value = progressBarQueue.Maximum;

                // music
                if (LambdaPlayItem != null)
                    await LambdaPlayItem.Invoke(_sid, _cid, cbi.Item, 1 + comboBoxAction.SelectedIndex);

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
                    _ticksToRefreshQueue = 20;
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
                _cid = last.Cid;
                _sid = last.Sid;
                await RefreshContainer(_sid, _cid);
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
            _sid = StartingPoints[si].Sid;
            _cid = StartingPoints[si].Cid;
            await RefreshContainer(_sid, _cid);
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
