using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChattingHub.Helper.Extensions
{
    public static class CollectionExtensions
    {
        public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> collection)
        {
            var ObservableCollection = new ObservableCollection<TSource>();
            foreach(var item in collection)
            {
                ObservableCollection.Add(item);
            }
            return ObservableCollection;
        }
    }
}
