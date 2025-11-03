using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _422_Tsyguleva_Pushkina
{
    public static class DbContextHelper
    {
        private static Tsyguleva_Pushkina_DB_PaymentEntities _context;

        public static Tsyguleva_Pushkina_DB_PaymentEntities GetContext()
        {
            if (_context == null)
                _context = new Tsyguleva_Pushkina_DB_PaymentEntities();
            return _context;
        }

        public static List<T> GetAll<T>() where T : class
        {
            return GetContext().Set<T>().ToList();
        }
    
        //public static void RemoveRange<T>(List<T> entities) where T : class
        //{
        //    using (var context = new Tsyguleva_Pushkina_DB_PaymentEntities())
        //    {
        //        context.Set<T>().RemoveRange(entities);
        //        context.SaveChanges();
        //    }
        //}
    }
}