newBooking.controller('NewBookingController', ['$scope', '$http', '$resource', NewBookingController]);

function NewBookingController($scope, $http, $resource) {
    CreateServices($resource);
    
    $scope.locations = Locations.query({});
    $scope.currentLocation = $scope.locations[0]
    $scope.currentRoom = '';

    $scope.$watch('currentLocation', function () { $('#calendar').fullCalendar('refetchEvents') });
    $scope.$watch('currentRoom', function () { $('#calendar').fullCalendar('refetchEvents') });
    
    InitiateCalendar();
}

function CreateServices($resource) {
    Locations = $resource('/api/locations', {       
    }, {
        query: { method: 'GET', isArray: true }
    })
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