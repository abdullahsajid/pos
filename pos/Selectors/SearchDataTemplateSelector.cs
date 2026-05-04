using pos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pos.Selectors
{
    public class SearchDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ProductTemplate { get; set; }
        public DataTemplate DealTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Models.SearchItemModel wrapper)
            {
                return wrapper.Item is Deal ? DealTemplate : ProductTemplate;
            }
            return item is Deal ? DealTemplate : ProductTemplate;
        }
    }
}
