newBooking.controller('NewBookingController', ['$scope', '$http', '$resource', NewBookingController]);

function NewBookingController($scope, $http, $resource) {
    CreateServices($resource);

    $scope.locations = Locations.query({});
    $scope.currentLocation = $scope.locations[0]


    //$scope.$watch('currentLocation', function () { $('#calendar').fullCalendar('refetchEvents') });
    //$scope.$watch('currentRoom', function () { $('#calendar').fullCalendar('refetchEvents') });
    //InitiateCalendar();

    $scope.GetRoomByName = function (roomName) {
        for (var i = 0; i < $scope.bookingFilter.location.rooms.length; i++) {
            if ($scope.bookingFilter.location.rooms[i].roomName == roomName) {
                return $scope.bookingFilter.location.rooms[i];
            }
        }
    }

    $scope.SearchBookings = function () {
        $scope.roomBookings = Available.query({
            location: $scope.bookingFilter.location.name,
            startDate: FormatDate($scope.bookingFilter.startDate),
            endDate: FormatDate($scope.bookingFilter.startDate),
            smartRoom: $scope.bookingFilter.smartRoom,
            numberOfAttendees: $scope.bookingFilter.numberOfAttendees
        }, function (success) {
            $("#bookingTable").html('');

            for (var i = 0; i < success.length; i++) {
                var eventData = [];
                for (var b = 0; b < success[i].bookings.length; b++) {
                    eventData.push({
                        title: success[i].bookings[b].owner,
                        start: new moment(success[i].bookings[b].startDate),
                        end: new moment(success[i].bookings[b].endDate)
                    });
                }
                var roomDetails = '<h2 style="text-align: center;">' + success[i].roomName + '</h2>';
                //debugger;
                roomDetails = roomDetails + '<span class="badge"><span class="glyphicon glyphicon-hdd" title="' + success[i].computerCount + ' PC\'s"> ' + success[i].computerCount + '</span></span>';
                roomDetails = roomDetails + '<span class="badge"><span class="glyphicon glyphicon-earphone" title="' + success[i].phoneCount + ' Phones"> ' + success[i].phoneCount + '</span></span>';

                $('#bookingTable').append('<div class="dailyCalendarContainer"><div style="text-align: center;">' + roomDetails + '</div><div id="' + success[i].roomName + '_calendar" class="dailyCalendar"></div></div>');
                var roomName = '[' + success[i].roomName + ']';
                $("#" + success[i].roomName + "_calendar").fullCalendar({
                    weekends: false,
                    header: {
                        left: '',
                        center: '',
                        right: ''
                    },
                    //defaultDate: $scope.bookingFilter.startDate,                    
                    defaultView: 'agendaDay',
                    selectable: true,
                    selectHelper: true,
                    height: 500,
                    titleFormat: '' + roomName + '',
                    select: function (start, end, allDay) {

                        var newEvent = { start: start, end: end };
                        if (isMeetingOverlapping(newEvent, this.calendar)) {
                            alert('New meeting clashes with existing booking. Please choose a new time!"');
                            return;
                        }

                        var $scope = angular.element($("#controllerDiv")).scope();
                        var room = $scope.GetRoomByName(this.title);

                        $scope.newBooking.roomName = room.roomName;

                        $scope.$digest();
                        $("#confirmModal").modal('show');
                    },
                    events: eventData
                });

            }

        });
    }

    $scope.bookingFilter = {
        startDate: new moment().utc().format('DD-MM-YYYY'),
        startTime: '',
        endTime: '',
        location: $scope.currentLocation,
        smartRoom: false,
        numberOfAttendees: 1
    };

    $scope.booking = {
        StartDate: new Date(),
        EndDate: new Date(),
        Room: { RoomName: '' },
        Location: { LocationName: '' }
    };

    $scope.newBooking = {
        roomName: '',
        internalAttendes: null,
        externalAttendes: null,
        pc: false,
        flipChart: false,
        projector: false
    };
}

function CreateServices($resource) {
    Locations = $resource('/api/locations', {
    }, {
        query: { method: 'GET', isArray: true }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom', { location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom' },
    {
        query: { method: 'GET', isArray: true }
    });
}

function InitiateCalendar() {
    $(function () {
        $('#calendar').fullCalendar({
            weekends: false,
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            events: function (start, end, timezone, callback) {
                var $scope = angular.element($("#controllerDiv")).scope();
                $.ajax({
                    url: '/api/bookings/' + moment.utc(start, "DD-MM-YYYY").format("MM-DD-YYYY") + '/' + moment.utc(end, "DD-MM-YYYY").format("MM-DD-YYYY") + '/' + $scope.currentRoom.roomName,
                    dataType: 'json',
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify($scope.currentLocation),
                    success: function (data) {
                        var events = [];
                        for (var i = 0; i < data.length; i++) {
                            events.push({
                                title: data[i].owner,
                                start: data[i].startDate,
                                end: data[i].endDate
                            });
                        }
                        callback(events);
                    }
                });

            }
        });
    });
}

function ExtractTimeFromDate(date) {
    var dateParts = date.split('T');

    var dateSplit = dateParts[0].split('-');
    var timeSplit = dateParts[1].split(':');

    var timeDate = new Date(dateSplit[0], dateSplit[1] - 1, dateSplit[2]);
    timeDate.setHours(timeSplit[0], timeSplit[1], timeSplit[2]);

    return timeDate;
}

function FormatDate(date) {

    if (date === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    var parts = date.split('-');
    myDate2 = new Date(parts[2], parts[1] - 1, parts[0]);

    return moment(myDate2).format('MM-DD-YYYY');
}

function FormatTime(time, date) {
    if (time === "") {
        alert('Invalid Time');
        throw new Error();
    }

    var parts = date.split('-');
    var timeSplit = convertTo24Hour(time).split(':');

    if (timeSplit[0] > 17 || (timeSplit[0] == 17 && timeSplit[1] > 0)) {
        alert('Time Must Be Before 5.00 PM');
        throw new Error();
    }
    else if (timeSplit[0] < 9) {
        alert('Time Must Be After 9.00 AM');
        throw new Error();
    };


    var timeDate = new Date(parts[2], parts[1] - 1, parts[0]);
    timeDate.setHours(timeSplit[0], timeSplit[1], 0);

    return timeDate;
}

function ValidateAttendess(attendees) {

    if (attendees === null) {
        throw new Error();
    }
    else if (isNaN(attendees)) {
        alert('Invalid Number');
        throw new Error();
    }
    else if (attendees <= 0) {
        alert('Attendees Can Not Be Negative');
        throw new Error();
    }
    return attendees;
}

function ValidateDates(start, end) {
    //ToDo: Add Equals then
    if (start > end) {
        alert('Start Date Can Not Be Ahead Of End Date');
        throw new Error();
    }
}

function convertTo24Hour(time) {
    var hours = parseInt(time.substr(0, 2));
    time = time.toString().trim();
    if (time.indexOf('AM') != -1 && hours == 12) {
        time = time.replace('12', '0');
    }
    if (time.indexOf('PM') != -1 && hours < 12) {
        time = time.replace(hours, (hours + 12));
    }
    return time.replace(/(AM|PM)/, '').slice(0, -1);
}

function IncrementTime(time, minutes) {
    var timeSplit = time.split(':');
    var amPm = timeSplit[1].split(" ");

    minutes = parseInt(amPm[0]) + minutes

    if (minutes >= 60) {
        minutes -= 60;
        ("0" + minutes).slice(-2);
    }

    return "'" + timeSplit[0] + ":" + minutes + " " + amPm[1] + "'";
}

function isMeetingOverlapping(event, calendar) {
    var array = calendar.clientEvents();
    for (i in array) {
        if (event.end > array[i].start && event.start < array[i].end) {
            return true;
        }
    }
    return false;
}