
/*mini fullcalendar for schedules*/

function loadCalendar(res_id, div_id) {
    $.getScript('~/Areas/RBM/Scripts/fullcalendar/dist/fullcalendar.js', function () {
        //script is loaded and executed put your dependent JS here

    var data = {
        resourceId: res_id
    };

    var date = new Date();
    var m = date.getMonth();
    var y = date.getFullYear();

    var calendar = $(div_id).fullCalendar({

        height: 200,
        theme: true,
        header:
       {
           left: 'prev,next today',
           center: 'title',
           right: ''
       },

        selectable: false,
        selectHelper: false,
        editable: false,

        events: function (start, end, timezone, callback) {
            $.ajax({
                url: '/RBM/Schedule/GetAllSchedulesFromResource',
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                //data: 'title=' + title + '&start=' + start + '&end=' + end,

                success: function (doc) {
                    var events = [];
                    $.each(doc, function (key, value) {
                        var val = $.parseJSON(value[0]);
                        events.push({
                            //title: val['title'],
                            start: val['start'],
                            end: val['end'],
                            color: val['color'],
                            eventId: val['eventId']
                        });
                    });

                    callback(events);
                }
            });
        }

    })
    });
}


//click the today button of the fullcalendar to open the calendar, workaround because there are problems with modal windows
function clickToday() {
    if (!$("tr.fc-week").is(':visible')) {
        $('.fc-today-button').click();
    }
}
