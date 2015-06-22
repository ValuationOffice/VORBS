support.controller('HelpController', ['$scope', '$http', '$resource', HelpController]);

function HelpController($scope, $http, $resource) {
    CreateHelpServices($resource);

    $scope.locations = Locations.query({},
        function (success) {
            $scope.currentLocation = $scope.locations[0];
        });

}

function CreateHelpServices($resource) {
    Locations = $resource('/api/locations', {}, {
        query: { method: 'GET', isArray: true }
    });
    Locations.prototype =
        {
            GetContactDetail: function (name) {
                for (var i = 0; i < this.locationCredentials.length; i++) {
                    if (this.locationCredentials[i].department == name) {
                        return this.locationCredentials[i];
                    }
                }
            }
        };
}

