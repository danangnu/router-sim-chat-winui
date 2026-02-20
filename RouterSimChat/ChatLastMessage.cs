using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RouterSimChat
{
    public class ChatLastMessage : INotifyPropertyChanged
    {
        private int _cardno;
        public int cardno { get => _cardno;
            set {
                if (_cardno != value) { 
                    _cardno = value;
                    OnPropertyChanged(nameof(isTrackITCard));
                }
            } }
        public string name { get; set; }
        public string number { get; set; }
        //public string message { get; set; }
        private string _message;
        public string message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(message));
                }
            }
        }
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusIcon));
                    OnPropertyChanged(nameof(isOutboxVisibility));
                }
            }
        }
        public string StatusIcon =>
        Status switch
        {
            "pending" => "⏳",
            "processing" => "✓",
            "sent" => "✓✓",
            "failed" => "❗",
            _ => ""
        };

        public Visibility isOutboxVisibility
        {
            get
            {
                if (Status == "")
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;

                }
            }
        }

        public Visibility isTrackITCard
        {
            get
            {
                if (_cardno == 0)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;

                }
            }
        }

        private DateTime _time;
        public DateTime time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged(nameof(time));
                    OnPropertyChanged(nameof(DisplayTime));
                }
            }
        }
        //public DateTime time { get; set; }
        public string box { get; set; }
        public int router_id { get; set; }
        //public int unread_count { get; set; }
        public string hostname { get; set; }
        public string call_interface { get; set; }
        public string DisplayDetail => $"Using {hostname} • {number}";
        private int _unread_count;
        public int unread_count
        {
            get => _unread_count;
            set
            {
                if (_unread_count != value)
                {
                    _unread_count = value;
                    OnPropertyChanged(nameof(unread_count));
                    OnPropertyChanged(nameof(UnreadVisibility));
                }
            }
        }

        public Visibility UnreadVisibility =>
    unread_count > 0 ? Visibility.Visible : Visibility.Collapsed;

    //        =>
    //box == "outbound" ? Visibility.Visible : Visibility.Collapsed;

        public string DisplayTime
        {
            get
            {
                var now = DateTime.Now;
                var date = time;

                if (date.Date == now.Date)
                {
                    // hari ini → jam
                    return date.ToString("HH:mm");
                }
                else if (date.Date == now.Date.AddDays(-1))
                {
                    // kemarin
                    return "Yesterday";
                }
                else
                {
                    // lebih lama → tanggal
                    return date.ToString("dd/MM/yyyy");
                }
            }
        }

        private bool _isActive;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(CardBrush));
                }
            }
        }
        public Brush CardBrush =>
       IsActive
           ? new SolidColorBrush(Colors.LightGray)
           : new SolidColorBrush(Colors.White);

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }

 }
