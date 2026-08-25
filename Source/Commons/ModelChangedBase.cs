using FZ4P.Commons.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons
{
    public class ModelChangedBase : Form,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 값 형식 반환 메서드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        protected void OnPropertyChanged<T>(string propertyName, T propertyValue) => PropertyChanged?.Invoke(this, new CustomPropertyChangedEventArgs<T>(propertyName, propertyValue));

        /// <summary>
        /// 인덱스,배열 형식 반환 메서드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        protected void OnPropertyIndexChanged<T>(int index, string propertyName, T propertyValue) => PropertyChanged?.Invoke(this, new ArrayChangedEventArgs<T>(index, propertyName, propertyValue));

        /// <summary>
        /// 일반적인 반환 메서드
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
