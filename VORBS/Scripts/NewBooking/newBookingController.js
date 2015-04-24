newBooking.controller('NewBookingController', ['$scope', '$http', '$resource', NewBookingController]);

function NewBookingController($scope, $http, $resource) {
    CreateServices($resource);
    
    $scope.locations = Locations.query({});
    $scope.currentLocation;
}

function CreateServices($resource) {
    Locations = $resource('/api/location', {       
    }, {
        query: { method: 'GET', isArray: true }
    })
}