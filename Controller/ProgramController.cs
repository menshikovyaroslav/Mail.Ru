using Mail.ru.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mail.ru.Controller
{
    public class ProgramController : INotifyPropertyChanged
    {
        public MainWindow MainWindow { get; set; }

        public ProgramController()
        {
            Session.Check();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
