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
            var receiver = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiver").ToString().Split(',').ToList();
            var receiverCC = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiverCC").ToString().Split(',').ToList();
            var receiverBCC = Modules.RBM.UI.Helper.Settings.get("BookingMailReceiverBCC").ToString().Split(',').ToList();

            var sender = Modules.RBM.UI.Helper.Settings.get("BookingMailSender").ToString();

            var subject = Modules.RBM.UI.Helper.Settings.get("BookingMailSubject").ToString() + ": " + bookingAction;

            string message = "";
            message += "<p>The following booking has been " + bookingAction + "</p>";
            message += "<table><tr><td>Booking name:</td><td>" + model.Name + "</td></tr>";
            if (model.Description != "")
                message += "<tr><td>Booking description:</td><td>" + model.Description + "</td></tr></table>";
            message += "<p>Booked Resources:</p>";
            message += "<table>";
            using (var userManager = new UserManager())
            using (var partyManager = new PartyManager())
            {

                foreach (ScheduleEventModel schedule in model.Schedules)
                {

                    message += "<tr><td>   </td><td>Resource:</td><td>" + schedule.ResourceName + "</td></tr>";
                    message += "<tr><td>   </td><td>Start date:</td><td>" + schedule.ScheduleDurationModel.StartDate.ToString("dd.MM.yyyy") + " </td></tr>";
                    message += "<tr><td>   </td><td>End date:</td><td>" + schedule.ScheduleDurationModel.EndDate.ToString("dd.MM.yyyy") + "</td></tr>";
                    message += "<tr><td>   </td><td>Reserved by:</td><td>" + schedule.ByPerson + "</td></tr>";
                    message += "<tr><td>   </td><td>Contact person:</td><td>" + schedule.ContactName + "( " + schedule.Contact.MobileNumber + ")</td></tr>";
                    message += "<tr><td>   </td><td>Reserved for:</td><td></td></tr>";
                    message += "<tr><td>   </td><td></td><td><ul>";


                    foreach (PersonInSchedule person in schedule.ForPersons)
                    {
                        message += "<li>" + person.UserFullName + "</li>";

                        var user = userManager.FindByIdAsync(person.UserId).Result;

                        if (user != null)
                        {
                            receiver.Add(user.Email);
                        }

                    }


                    message += "</ul></td></tr>";
                }
            }
            message += "</table>";

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