using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Antiban
{
    public class Antiban
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        /// <summary>
        /// Добавление сообщений в систему, для обработки порядка сообщений
        /// </summary>
        /// <param name="eventMessage"></param>
        public void PushEventMessage(EventMessage eventMessage)
        {
            var currentPhone = _context.EventMessages.Where(e => e.Phone == eventMessage.Phone)
                                                     .OrderBy(e => e.DateTime).ToList();

            var lastTodayEM = _context.EventMessages.Where(e => e.DateTime.Day == 1 
                                                     && e.Phone != eventMessage.Phone
                                                     && e.Priority == 0)
                                                     .OrderByDescending(e => e.DateTime).FirstOrDefault();

            if (eventMessage.Priority == 1 && currentPhone != null && currentPhone.Where(e => e.Priority == 1).Any())
            {
				//период между сообщениями с приоритетом=1(- рассылки) на один номер, не менее 24 часа. Т.е. должна отправлять только одна рассылка в день на один номер.
                if(currentPhone.Count > 0 && eventMessage.DateTime < currentPhone.Where(e => e.Priority == 1).Last().DateTime.AddDays(1))
					eventMessage.DateTime = currentPhone.Where(e => e.Priority == 1).Last().DateTime.AddDays(1);

			}
            else if (currentPhone != null && currentPhone.Count > 0 && currentPhone.Where(e => e.DateTime.Day == 1).Last().DateTime.AddMinutes(1) >= eventMessage.DateTime)
            {
				//период между сообщениями на один номер, должен быть не менее 1 минуты. Для теста возьмем, что равно 1 минуте
				eventMessage.DateTime = currentPhone.Where(e => e.DateTime.Day == 1).Last().DateTime.AddMinutes(1);

			}
            else if (lastTodayEM != null && lastTodayEM.DateTime.AddSeconds(10) >= eventMessage.DateTime)
            {
				//период между сообщениями на разные номера, должен быть не менее 10 сек. Для теста возьмем, что равно 10 сек
				eventMessage.DateTime = lastTodayEM.DateTime.AddSeconds(10);

			}

            _context.EventMessages.Add(eventMessage);
            _context.SaveChanges();            
		}

		/// <summary>
		/// Вовзращает порядок отправок сообщений
		/// </summary>
		/// <returns></returns>
		public List<AntibanResult> GetResult()
        {
            var result = _context.EventMessages.OrderBy(e => e.DateTime).Select(e => new AntibanResult
            {
                EventMessageId = e.Id,
                SentDateTime = e.DateTime

            }).ToList();

			return result;
        }
    }
}
