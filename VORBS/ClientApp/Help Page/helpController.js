(function () {
    angular.module('vorbs.supportPage')
        .controller('HelpController', HelpController);


    HelpController.$inject = ['$scope', 'LocationsService'];

    function HelpController($scope, LocationsService) {

        $scope.locationCredentials = {
            facilities: {
                phoneNumber: '',
                email: ''
            },
            dss: {
                phoneNumber: '',
                email: ''
            }
        };

        $scope.locations = LocationsService.getByStatus({ status: true, extraInfo: true }).$promise.then(
            function (resp) {
                $scope.locations = resp;
                $scope.currentLocation = $scope.locations[0];
                $scope.getHelpData();
            }
        );

        $scope.getHelpData = function () {
            $scope.locationCredentials.facilities = $scope.currentLocation.locationCredentials.filter(function (value) {
                return value.department == 'facilities';
            })[0];

            $scope.locationCredentials.dss = $scope.currentLocation.locationCredentials.filter(function (value) {
                return value.department == 'dss';
            })[0];
        }
    }

})();