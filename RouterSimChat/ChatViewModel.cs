using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Input;
using WinRT;

namespace RouterSimChat
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ChatItem> Messages { get; set; } = new ObservableCollection<ChatItem>();
        public ObservableCollection<ChatLastMessage> chatDetails { get; set; } = new ObservableCollection<ChatLastMessage>();
        public ObservableCollection<ChatLastMessage> LastMessages { get; set; } = new ObservableCollection<ChatLastMessage>();
        public List<ChatLastMessage> _allLastMessages = new();
        public ObservableCollection<RouteDevice> Devices { get; set; } = new ObservableCollection<RouteDevice>();
        private List<RouteDevice> _allDevices = new();
        public ObservableCollection<RouteAgent> Agents { get; set; } = new ObservableCollection<RouteAgent>();
        private List<Contact> _allContacts = new();
        public ObservableCollection<StaffLogin> staffDetails { get; set; } = new ObservableCollection<StaffLogin>();
        private List<StaffLogin> _staffDetails = new();
        private static readonly HttpClient client = new HttpClient();
        private DispatcherTimer _refreshTimer;
        public event Action RequestScrollToBottom;
        public ObservableCollection<object> SearchResults { get; set; } = new ObservableCollection<object>();

        //private string _baseUrl;

        //public void getBaseUrl(string baseUrl)
        //{
        //    _baseUrl = baseUrl?.TrimEnd('/');
        //}
        private readonly string _baseUrl;

        public ChatViewModel(string baseUrl)
        {
            _baseUrl = baseUrl?
                .Trim()
                .TrimEnd('/');
        }
        private void HandleDeviceChanged()
        {
            if (SelectedDevice?.router_id > 0)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }
        private bool _isRefreshing;
        public void StartAutoRefresh()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new DispatcherTimer();
                _refreshTimer.Interval = TimeSpan.FromSeconds(15); // 1 menit terlalu lama untuk chat

                _refreshTimer.Tick += async (s, e) =>
                {
                    if (_isRefreshing)
                        return;

                    _isRefreshing = true;

                    try
                    {
                        
                        await refreshMessagesAsync();
                        if (SelectedDevice?.router_id > 0)
                        {
                            await checkRuterAgent(SelectedDevice.router_id);
                        }
                        if (DevideStaffID == 0)
                        {
                            await checkRouteStatus();
                        }
                        if (SelectedMsg != null)
                        {
                            await RefreshActiveChatAsync();
                        }
                    }
                    finally
                    {
                        _isRefreshing = false;
                    }
                };
            }

            _refreshTimer.Start();
        }
        public async Task checkRouteStatus()
        {
            try
            {
                var data2 = await client.GetFromJsonAsync<List<RouteDevice>>($"{_baseUrl}/api/routers/device?router_id=0");

                if (data2 != null && data2.Any())
                {
                    foreach (var newDevice in data2)
                    {
                        var existingDevice = Devices
                            .FirstOrDefault(x => x.router_id == newDevice.router_id);

                        if (existingDevice != null)
                        {
                            // update property dari API
                            existingDevice.last_heartbeat = newDevice.last_heartbeat;
                            existingDevice.status = newDevice.status;
                            existingDevice.hostname = newDevice.hostname;

                            // hitung ulang state
                            existingDevice.statusHeartbeat =
                                DateTime.Now - existingDevice.last_heartbeat <= TimeSpan.FromMinutes(2)
                                    ? "up"
                                    : "down";

                            existingDevice.StatusBrush_color =
                                existingDevice.statusHeartbeat == "up"
                                    ? new SolidColorBrush(Colors.LightGreen)
                                    : new SolidColorBrush(Colors.Red);

                            existingDevice.checkStr =
                                existingDevice.statusHeartbeat == "up"
                                    ? "✓"
                                    : "";

                            existingDevice.isUp =
                                existingDevice.statusHeartbeat == "up";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }
        private int _staffRouterID = 0;
        public async Task checkRuterAgent(int deviceId)
        {
            try
            {
                var data = await client.GetFromJsonAsync<List<RouteAgent>>($"{_baseUrl}/api/routers/agent?router_id={deviceId}");

                if (data != null && data.Any())
                {
                    var agent = data.First();

                    foreach (var staff in staffDetails)
                    {
                        if (_staffRouterID == agent.router_id)
                        {
                            staff.RouteDevice = agent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }
        public async Task RefreshActiveChatAsync()
        {
            //if (SelectedDevice?.router_id == null)
            //    return;
            try
            {
                //var data = await client.GetFromJsonAsync<List<ChatLastMessage>>($"{_baseUrl}/api/messages/lastchat?router_id={SelectedDevice.router_id}");

                //if (data != null)
                //{
                //    _allLastMessages = data;
                //    foreach (var newMsg in data)
                //    {
                //        var existing = LastMessages
                //            .FirstOrDefault(x => x.number == newMsg.number);

                //        if (existing != null)
                //        {
                //            // Update property saja
                //            existing.message = newMsg.message;
                //            existing.unread_count = newMsg.unread_count;
                //            existing.time = newMsg.time;
                //        }
                //        else
                //        {
                //            LastMessages.Add(newMsg);
                //        }
                //    }
                //}

                var data2 = await client.GetFromJsonAsync<List<ChatMessage>>($"{_baseUrl}/api/messages/history?router_id={SelectedMsg.router_id}&peer={SelectedMsg.number}&limit=50");
                DateTime? lastDate = null;
                if (data2 != null)
                {
                    DateTime? lastMessageTimeBefore = Messages
                    .OfType<ChatMessage>()
                    .LastOrDefault()?.Time;
                    Messages.Clear();
                    foreach (var msg in data2.OrderBy(x => x.Time))
                    {
                        var currentDate = msg.Time.Date;

                        if (lastDate == null || currentDate != lastDate)
                        {
                            Messages.Add(new ChatDateSeparator
                            {
                                DateHeaderText = currentDate == DateTime.Today
                                    ? "Today"
                                    : currentDate == DateTime.Today.AddDays(-1)
                                        ? "Yesterday"
                                        : currentDate.ToString("dd MMM yyyy")
                            });

                            lastDate = currentDate;
                        }

                        Messages.Add(msg);
                    }
                    var lastMessageTimeAfter = data2
                    .OrderBy(x => x.Time)
                    .LastOrDefault()?.Time;

                    if (lastMessageTimeAfter > lastMessageTimeBefore)
                    {
                        RequestScrollToBottom?.Invoke();
                    }

                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public void StopAutoRefresh()
        {
            _refreshTimer?.Stop();
        }
        public async Task LoadMessagesAsync(int router_id, string peer)
        {
            try
            {
                var data = await client.GetFromJsonAsync<List<ChatLastMessage>>($"{_baseUrl}/api/messages/lastchatDetail?router_id={router_id}&peer={peer}");

                if (data != null)
                {
                    chatDetails.Clear();
                    foreach (var msgDetail in data)
                    {
                        if (_allContacts.Count == 0)
                        {
                            await LoadContact();
                        }

                        var contact = _allContacts.FirstOrDefault(c =>
                            NormalizeNumber(c.contact) ==
                            NormalizeNumber(msgDetail.number));

                        if (contact != null) { 
                            msgDetail.name = contact.name;
                            msgDetail.cardno = contact.cardno;
                        }
                        else { 
                            msgDetail.name = msgDetail.number;
                            msgDetail.cardno = 0;
                        }
                        CurrentPeer = msgDetail.number;
                        CurrentCardNo = msgDetail.cardno;
                        chatDetails.Add(msgDetail);
                    }
                       
                }

                var data2 = await client.GetFromJsonAsync<List<ChatMessage>>($"{_baseUrl}/api/messages/history?router_id={router_id}&peer={peer}&limit=50");
                DateTime? lastDate = null;
                if (data2 != null)
                {
                    Messages.Clear();
                    foreach (var msg in data2.OrderBy(x => x.Time))
                    {
                        var currentDate = msg.Time.Date;

                        if (lastDate == null || currentDate != lastDate)
                        {
                            Messages.Add(new ChatDateSeparator
                            {
                                DateHeaderText = currentDate == DateTime.Today
                                    ? "Today"
                                    : currentDate == DateTime.Today.AddDays(-1)
                                        ? "Yesterday"
                                        : currentDate.ToString("dd MMM yyyy")
                            });

                            lastDate = currentDate;
                        }

                        Messages.Add(msg);
                    }
                }

            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }
          
        }


        public async Task LoadLastMessagesAsync(int deviceId)
        {
            try
            {
                
                var data = await client.GetFromJsonAsync<List<ChatLastMessage>>($"{_baseUrl}/api/messages/lastchat?router_id={deviceId}");

                if (data != null)
                {
                    LastMessages.Clear();
                    _allLastMessages = data;
                    foreach (var msg in data)
                        LastMessages.Add(msg);
                    FilterMsg();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public async Task opencardTI()
        {
            try
            {

                string id = CurrentCardNo.ToString();
                await client.GetAsync($"http://127.0.0.1:54321/?id={id}");
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public async Task  loadStaffTrackit()
        {
            try
            {
                string username = Environment.UserName;
                var data = await client.GetFromJsonAsync<List<StaffLogin>>($"{_baseUrl}/api/routers/trackit_login?username={username}");

                if (data != null)
                {
                    _staffDetails = data;
                    foreach (var msg in data)
                    {
                        DevideStaffID = msg.router_id;
                        await LoadContact();
                        //await LoadLastMessagesAsync(msg.router_id);
                        await LoadDeviceAsync(msg.router_id);

                        if(msg.route_name == null)
                        {
                            msg.route_name = "Administrator";
                        }

                        staffDetails.Add(msg);
                    }
                        
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public async Task LoadContact()
        {
            try
            {
                 
                var data = await client.GetFromJsonAsync<List<Contact>>($"{_baseUrl}/api/contact");

                if (data != null)
                {
                    _allContacts = data;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public async Task refreshMessagesAsync()
        {
            if (SelectedDevice?.router_id == null)
                return;
            try
            {
                var data = await client.GetFromJsonAsync<List<ChatLastMessage>>($"{_baseUrl}/api/messages/lastchat?router_id={SelectedDevice.router_id}");
                bool isSorting = false;

                if (data != null)
                {
                    //LastMessages.Clear();
                    foreach (var newMsg in data.OrderByDescending(x => x.time))
                    {
                        var existing = LastMessages
                            .FirstOrDefault(x => x.number == newMsg.number);
                        bool newData = false;

                        if (existing != null) {
                            bool isUpdated =
    existing.message != newMsg.message ||
    existing.time != newMsg.time ||
    existing.unread_count != newMsg.unread_count ||
    existing.Status != newMsg.Status;
                            if (isUpdated)
                            {
                                newData = true;
                                // Update property saja
                                //existing.message = newMsg.message;
                                //existing.unread_count = newMsg.unread_count;
                                //existing.time = newMsg.time;
                                // update isi
                                int oldIndex = LastMessages.IndexOf(existing);
                                
                                existing.number = newMsg.number;
                                existing.message = newMsg.message;
                                existing.unread_count = newMsg.unread_count;
                                existing.time = newMsg.time;
                                existing.Status = newMsg.Status;
                                // pindahkan ke atas
                                //LastMessages.Move(oldIndex, 0);
                                isSorting = true;
                                if (newMsg.box == "inbound")
                                {
                                    ShowToast(newMsg.number, newMsg.message);
                                }
                                //if (newMsg.unread_count > 0)
                                //{
                                //}
                            }
                        }
                        else
                        {
                            LastMessages.Insert(0, newMsg);
                            if(newMsg.box == "inbound")
                            {
                                ShowToast(newMsg.number, newMsg.message);
                            }
                            isSorting = true;
                        }
                        //LastMessages.Add(newMsg);

                    }

                    var sorted = LastMessages
    .OrderByDescending(x => x.time)
    .ToList();
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        int currentIndex = LastMessages.IndexOf(sorted[i]);
                        if (currentIndex != i)
                        {
                            LastMessages.Move(currentIndex, i);
                        }
                    }
                    _allLastMessages = LastMessages.ToList();
                    if (isSorting)
                    {
                        FilterMsg();

                    }
                    else
                    {
                        // 🔥 TAMBAHKAN INI
                        if (!string.IsNullOrWhiteSpace(SearchTextMsg))
                        {
                            FilterMsg();
                        }
                    }
                    

                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public void ShowToast(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddArgument("action", "openChat")
                .AddArgument("sender", title)
                .Show();
        }

        private int _devide_staffID;

        public int DevideStaffID
        {
            get => _devide_staffID;
            set
            {
                if (_devide_staffID != value)
                {
                    _devide_staffID = value;

                    OnPropertyChanged(nameof(FirstColumnWidth));
                }
            }
        }

        public GridLength FirstColumnWidth =>
        DevideStaffID > 0
        ? new GridLength(0)
        : new GridLength(380);

        public async Task LoadDeviceAsync(int deviceId)
        {
            try
            {
                var data = await client.GetFromJsonAsync<List<RouteDevice>>($"{_baseUrl}/api/routers/device?router_id={deviceId}");

                if (data != null)
                {
                    Devices.Clear();
                    _allDevices = data;
                    foreach (var dt in data)
                    {

                        // hitung brush setelah status di-set
                        dt.statusHeartbeat = DateTime.Now - dt.last_heartbeat <= TimeSpan.FromMinutes(2)
                            ? "up"
                            : "down";
                        dt.StatusBrush_color = dt.statusHeartbeat == "up"
                            ? new SolidColorBrush(Colors.LightGreen)   // hijau
                            : new SolidColorBrush(Colors.Red);    // merah
                        dt.checkStr = dt.statusHeartbeat == "up"
                            ? "✓"
                            : "";    // merah
                        dt.isUp = dt.statusHeartbeat?.ToLower() == "up";

                        Devices.Add(dt);
                    }


                    // ✅ AUTO SELECT DI SINI
                    if (Devices.Any())
                    {
                        SelectedDevice = Devices[0];
                        await checkRuterAgent(SelectedDevice.router_id);
                        _staffRouterID = SelectedDevice.router_id;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    FilterDevices();
                }
            }
        }
        private void FilterDevices()
        {
            try
            {
                Devices.Clear();
                var filtered = string.IsNullOrWhiteSpace(SearchText)
                    ? _allDevices
                     : _allDevices.Where(x =>
        (x.hostname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (x.sim?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
      ).ToList();
                foreach (var item in filtered)
                    Devices.Add(item);
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }
           
        }


        private string _searchTextMsg;
        public string SearchTextMsg
        {
            get => _searchTextMsg;
            set
            {
                if (_searchTextMsg != value)
                {
                    _searchTextMsg = value;
                    FilterMsg();
                }
            }
        }
        private string NormalizeNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return string.Empty;

            number = new string(number.Where(char.IsDigit).ToArray());

            if (number.StartsWith("0"))
                number = "61" + number.Substring(1); // sesuaikan kode negara

            return number;
        }

        private async Task FilterMsg()
        {
            try
            {
                SearchResults.Clear();

                if (string.IsNullOrWhiteSpace(SearchTextMsg))
                {
                    //foreach (var item in _allLastMessages.OrderByDescending(x => x.time))
                    //    SearchResults.Add(item);
                    foreach (var item in _allLastMessages.OrderByDescending(x => x.time))
                    {
                        if (_allContacts.Count == 0)
                        {
                            await LoadContact();
                        }

                        var contact = _allContacts.FirstOrDefault(c =>
                            NormalizeNumber(c.contact) ==
                            NormalizeNumber(item.number));

                        if (contact != null)
                        {
                            item.name = contact.name;
                            item.cardno = contact.cardno;
                        }
                        else
                        {
                            item.name = item.number; // fallback kalau tidak ada contact
                            item.cardno = 0; // fallback kalau tidak ada contact
                        }
                           
                           

                        SearchResults.Add(item);
                    }
                    return;
                }
                // 1️⃣ Search di chat
                //var chats = _allLastMessages.OrderByDescending(x => x.time)
                //    .Where(x =>
                //        (x.number?.Contains(SearchTextMsg,
                //        StringComparison.OrdinalIgnoreCase)) ?? false)
                //    .ToList();
                var search = SearchTextMsg?.Trim();
                var chats = _allLastMessages
                .OrderByDescending(x => x.time)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.number) &&
                     x.number.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ||
                    (!string.IsNullOrEmpty(x.name) &&
                     x.name.Contains(search, StringComparison.OrdinalIgnoreCase))
                )
                .ToList();
                foreach (var chat in chats)
                    SearchResults.Add(chat);

                // 2️⃣ Cari di contact (yang belum ada di chat)
                var contacts = _allContacts
                    .Where(c =>
                        (c.name.Contains(SearchTextMsg, StringComparison.OrdinalIgnoreCase)
                         || c.contact.Contains(SearchTextMsg))
                        && !_allLastMessages.Any(l => NormalizeNumber(l.number) == NormalizeNumber(c.contact)))
                    .ToList();

                foreach (var contact in contacts)
                    SearchResults.Add(contact);
                // 3️⃣ Kalau tidak ada sama sekali dan input angka
                if (!SearchResults.Any() && SearchTextMsg.All(char.IsDigit))
                {
                    SearchResults.Add(new Contact
                    {
                        name = SearchTextMsg,
                        contact = SearchTextMsg
                    });
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }

        }

        public async Task MarkAsReadAsync(ChatLastMessage item)
        {
            try
            {
                // hit API update
                await client.PostAsync(
                    $"{_baseUrl}/api/messages/mark-read?router_id={item.router_id}&peer={item.number}",
                    null);

                // update local UI
                item.unread_count = 0;
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }
        }
        public string NormalizePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // 1️⃣ Hapus semua selain digit
            var digits = new string(input.Where(char.IsDigit).ToArray());

            // 2️⃣ Kalau mulai dengan 0 → ganti jadi 62
            if (digits.StartsWith("0"))
                digits = "61" + digits.Substring(1);

            // 3️⃣ Kalau mulai dengan 62 → biarkan
            else if (digits.StartsWith("61"))
                return digits;

            // 4️⃣ Kalau mulai dengan 8 (tanpa 0) → tambahkan 62
            else if (digits.StartsWith("4"))
                digits = "61" + digits;

            return digits;
        }
        //public event PropertyChangedEventHandler? PropertyChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //private RouteDevice _selectedDevice;
        private RouteDevice _selectedDevice;
        public RouteDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    if (_selectedDevice != null)
                        _selectedDevice.IsActive = false;

                    _selectedDevice = value;

                    if (_selectedDevice != null)
                    {
                        _selectedDevice.IsActive = true;

                        // 🔥 reload last message berdasarkan router
                        
                        _ = LoadLastMessagesAsync(_selectedDevice.router_id);
                        HandleDeviceChanged();
                    }
                    OnPropertyChanged(nameof(SelectedDevice));
                }
            }
        }

        public async Task SelectDeviceAsync(RouteDevice device)
        {
            SelectedDevice = device;

            if (device != null)
            {
                await LoadLastMessagesAsync(device.router_id);
                _staffRouterID = device.router_id;
                await checkRuterAgent(_staffRouterID);
            }
        }

        private ChatLastMessage _SelectedMsg;
        public ChatLastMessage SelectedMsg
        {
            get => _SelectedMsg;
            set
            {
                if (_SelectedMsg != value)
                {
                    if (_SelectedMsg != null)
                        _SelectedMsg.IsActive = false;

                    _SelectedMsg = value;

                    if (_SelectedMsg != null)
                    {
                        _SelectedMsg.IsActive = true;

                        // 🔥 reload last message berdasarkan router
                        _ = LoadMessagesAsync(_SelectedMsg.router_id,_SelectedMsg.number);
                    }

                    OnPropertyChanged(nameof(SelectedMsg));
                }
            }
        }

        public async Task SelectMessageAsync(ChatLastMessage msg)
        {
            SelectedMsg = msg;

            if (msg != null)
            {
                await LoadMessagesAsync(msg.router_id, msg.number);
            }
        }
        private string _messageText;
        public string MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged(nameof(MessageText));
                }
            }
        }

        public string CurrentPeer { get; set; }
        public int CurrentCardNo { get; set; }
        public async Task SendSmsAsync()
        {
            try
            {
                var request = new SendSmsRequest
                {
                    router_id = SelectedDevice.router_id,
                    slot = SelectedDevice.call_interface,
                    peer = NormalizePhoneNumber(CurrentPeer),
                    body = MessageText
                };

                var response = await client.PostAsJsonAsync(
                    $"{_baseUrl}/api/send-sms",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    // Optional: ambil response
                    var result = await response.Content.ReadFromJsonAsync<SendSmsOut>();

                    // Tambahkan ke chat UI
                    Messages.Add(new ChatMessage
                    {
                        Message = MessageText,
                        Box = "outbound",
                        Time = DateTime.Now,
                        Status = "pending"
                    });

                    MessageText = ""; // clear textbox
                   await refreshMessagesAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ExceptionHelpers.SetErrorInfo(new Exception(error));
                }
            }
            catch (Exception ex)
            {
                ExceptionHelpers.SetErrorInfo(ex);
            }
        }

        public void OpenEmptyChat(Contact contact)
        {
            // Kosongkan header lama
            chatDetails.Clear();

            // Isi header sesuai contact
            chatDetails.Add(new ChatLastMessage
            {
                number = contact.contact,
                name = contact.name,
                router_id = SelectedDevice.router_id,
                hostname = SelectedDevice.hostname,
                message = "",
                unread_count = 0,
                time = DateTime.Now
            });
            // Kosongkan history
            Messages.Clear();

            // Set peer aktif
            CurrentPeer = contact.contact;

            // Reset selected message supaya tidak dianggap chat lama
            SelectedMsg = null;
            SearchTextMsg = "";
        }
        public async Task OpenChatAsync(ChatLastMessage msg)
        {
            SearchTextMsg = "";
            Messages.Clear();
            chatDetails.Clear();

            await MarkAsReadAsync(msg);
            await SelectMessageAsync(msg);
        }
        public async Task OpenChatBySenderAsync(string sender)
        {
            var msg = _allLastMessages.FirstOrDefault(x => x.number == sender);

            if (msg != null)
            {
                await OpenChatAsync(msg);
            }
        }
    }
}
