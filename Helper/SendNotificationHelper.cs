using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Areas.RBM.Helpers
{
    public class SendNotificationHelper
    {
        public enum BookingAction
        {
            created,
            edited,
            deleted
        }


        public static void SendNotification(List<string> receiver, string message, string subject)
        {
            List<string> receiverBCC = new List<string>();
            receiverBCC.Add(ConfigurationManager.AppSettings["SystemEmail"]);

            var emailService = new EmailService();
            emailService.Send(subject, message, receiver, null, receiverBCC, null);
        }

        /// <summary>
        /// Send a booking notification (create, edit or delete event) as email to in config file defined receivers
        /// </summary>
        /// <param name="bookingAction">For which action the notification will be send. (create, edit or delete event)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static void SendBookingNotification(BookingAction bookingAction, BookingEventModel model)
        {
            //get receiver and sender from the settings file
            var settings = ModuleManager.GetModuleSettings("rbm");
            var receiver = settings.GetValueByKey("BookingMailReceiver").ToString().Split(',').ToList();
            var receiverCC = settings.GetValueByKey("BookingMailReceiverCC").ToString().Split(',').ToList();
            var receiverBCC = settings.GetValueByKey("BookingMailReceiverBCC").ToString().Split(',').ToList();

            //var sender = Modules.RBM.UI.Helper.Settings.get("BookingMailSender").ToString();

            var subject = settings.GetValueByKey("BookingMailSubject").ToString() + ": " + bookingAction;

            string message = "";
            message += "<p>The following booking has been " + bookingAction + "</p>";
            message += "<b>Booking name: </b>" + model.Name + "</br>";
            if (!String.IsNullOrEmpty(model.Description))
                message += "<b>Booking description: </b> " + model.Description + "</br>";
            message += "<p><b>Booked Resources:</b></p>";
            using (var userManager = new UserManager())
            using (var partyManager = new PartyManager())
            {
                foreach (ScheduleEventModel schedule in model.Schedules)
                {
                    message += "<b>Resource: </b>" + schedule.ResourceName + "</br>";
                    message += "<b>Start date: </b>" + schedule.ScheduleDurationModel.StartDate.ToString("dd.MM.yyyy") + "</br>";
                    message += "<b>End date: </b>" + schedule.ScheduleDurationModel.EndDate.ToString("dd.MM.yyyy") + "</br>";
                    message += "<b>Reserved by: </b>" + schedule.ByPerson + "</br>";
                    message += "<b>Contact person: </b>" + schedule.ContactName + " ( #" + schedule.Contact.MobileNumber + ")</br>";
                    message += "<b>Reserved for: </b>";


                    foreach (PersonInSchedule person in schedule.ForPersons)
                    {
                        if (schedule.ForPersons.IndexOf(person) == schedule.ForPersons.Count - 1)
                            message += person.UserFullName;
                        else
                            message += person.UserFullName + ", ";

                        var user = userManager.FindByIdAsync(person.UserId).Result;

                        if (user != null)
                        {
                            receiver.Add(user.Email);
                        }

                    }

                    message += "</br></br>";

                }
            }

            receiverBCC.Add(ConfigurationManager.AppSettings["SystemEmail"].ToString()); // Allways send BCC to SystemEmail 

            var emailService = new EmailService();
            emailService.Send(
               subject,
               message,
               receiver.Distinct().ToList(), 
               receiverCC, 
               receiverBCC 
               );
        }

    }
}