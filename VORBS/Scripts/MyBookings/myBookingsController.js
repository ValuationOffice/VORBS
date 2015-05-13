myBookings.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateServices($resource);

    //TimeQuery.query({},
    //function (success) {
    //    alert("Success");
    //},
    //function (error) {
    //    alert(error);
    //}
    //);

    $scope.bookings = Booking.query({});
}


function CreateServices($resource) {
    //Only retreives bookings in the next 3 months. TODO: Confirm ?
    Booking = $resource('/api/bookings/' + new moment().format("MM-DD-YYYY") + '/' + new moment().add('week', 12).format("MM-DD-YYYY"), {
    }, {
        query: { method: 'GET', isArray: true }
    });

    Booking.prototype = {
        startDateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY - hh:mm A"); },
        endDateFormatted: function () { return moment(this.endDate).format("DD/MM/YYYY - hh:mm A"); }
    };

    TimeQuery = $resource('/api/bookings/' + encodeURIComponent("2015-02-04T05:10:58+05:30"), {
    }, {
        query: { method: 'GET', isArray: true }
    });
}