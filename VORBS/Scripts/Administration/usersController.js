(function () {

    angular.module('vorbs.admin')
        .controller('UsersController', UsersController);

    UsersController.$inject = ['$scope', 'LocationsService', 'UsersService', 'AdminsService'];

    function UsersController($scope, LocationsService, UsersService, AdminsService) {

        $scope.Locations = LocationsService.query().$promise.then(function (resp) {
            $scope.Locations = resp;
        });

        $scope.admins = [];

        $scope.adminId = 0;

        $scope.SetAdminId = function (id) {
            $scope.adminId = id;
        }

        $scope.AddAdmin = function () {
            SetAdminErrorMessage('');

            //Validate PID
            if ($scope.adminUser.PID === "") {
                SetAdminErrorMessage('Invalid PID.');
                return;
            }

            //Validate Location
            if ($scope.adminUser.LocationID === undefined) {
                SetAdminErrorMessage('Invalid location.');
                return;
            }
            else {
                $scope.adminUser.LocationID = $scope.adminUser.LocationID;
            }

            AdminsService.save({}, $scope.adminUser)
                .$promise.then(function (resp) {
                    alert('User Added Successfully!');
                    $scope.CreateNewUserObject();
                    getAdmins();
                }, function (error) {
                    if (error.status == 409) {
                        SetAdminErrorMessage('Administrator already exists.');
                    }
                    else {
                        alert('Unable to Add Admin. Please Contact ITSD.');
                    }
                }).finally(function () {
                    SetAdminErrorMessage('');
                });
        }


        var editUserDetails = {
            locationId: 0,
            permissionLevel: 0
        };
        $scope.GetAdmin = function (id) {
            $scope.SetAdminId(id);

            var indexAdmin = GetIndexFromAdmins($scope.admins, $scope.adminId);
            if (indexAdmin >= 0) {
                $scope.editAdmin = angular.copy($scope.admins[indexAdmin]);

                editUserDetails = {
                    locationId: $scope.editAdmin.location.id,
                    permissionLevel: $scope.editAdmin.permissionLevel
                };
            }
            else {
                alert('Unable to retrieve admin data. Admin may have already been deleted.');
                $('#editAdminModal').modal('hide');
            }
        }

        $scope.EditAdmin = function (id) {
            var dataChanged = false;
            //Change the "edit booking" button to stop multiple edits
            $("#editAdminEditButton").prop('disabled', 'disabled');
            $("#editAdminEditButton").html('Editing Admin. Please wait..');

            if ($scope.editAdmin.permissionLevel != editUserDetails.permissionLevel
                || $scope.editAdmin.location.id != editUserDetails.locationId) {
                $scope.editAdmin.LocationID = $scope.editAdmin.location.id;
                dataChanged = true;
            }

            if (!dataChanged) {
                alert('Admin data has not changed.');
                EnableEditAdminButton();
                return;
            }

            AdminsService.update({ id: $scope.adminId }, $scope.editAdmin)
                .$promise.then(function () {
                    alert('Admin Edited Successfully!');
                    getAdmins();
                    $('#editAdminModal').modal('hide');
                }, function () {
                    alert('Unable to edit the admin. Please try again or contact ITSD.');
                }).finally(function () {
                    EnableEditAdminButton();
                });
        }

        $scope.DeleteAdmin = function () {
            AdminsService.delete({ id: $scope.adminId }).$promise.then(function () {
                getAdmins();
                $('#deleteAdminModal').modal('hide');
            }, function () {
                alert('Unable to delete the admin. Please try again or contact ITSD.');
            });
        };

        $('#activeDirecotryModal').on('show.bs.modal', function () {
            $scope.GetAdNames();
        });

        $scope.GetAdNames = function () {
            if ($scope.emailVal === undefined || $scope.emailVal.trim() === "") {
                UsersService.query().$promise.then(function (resp) {
                    $scope.adAdminUsers = resp;
                });
            }
            else {
                UsersService.get({
                    name: $scope.emailVal
                }).$promise.then(function (resp) {
                    $scope.adAdminUsers = resp;
                });
            }
        }

        $scope.AddAdEmail = function (user) {
            $scope.adminUser.PID = user.pid;
            $scope.adminUser.FirstName = user.firstName;
            $scope.adminUser.LastName = user.lastName;
            $scope.adminUser.Email = user.email;

            $('#activeDirecotryModal').modal('hide');
        }

        $scope.CreateNewUserObject = function () {
            $scope.adminUser = {
                PID: '',
                FirstName: '',
                LastName: '',
                LocationID: '',
                Email: '',
                PermissionLevel: 1
            }
        }

        $scope.CreateNewUserObject();
        $scope.adAdminUser = {
            pID: '',
            firstName: '',
            lastName: '',
            email: ''
        };

        function getAdmins() {

            var result = [];

            AdminsService.query().$promise.then(function (resp) {
                result = resp;
            }).finally(function () {
                $scope.admins = result;
            });
        }

        getAdmins();
    }



    function EnableEditAdminButton() {
        //Change the "new booking" button to stop multiple bookings
        $("#editAdminEditButton").prop('disabled', '');
        $("#editAdminEditButton").html('Accept Changes');
    }

    function SetAdminErrorMessage(message) {
        if (message === "") {
            $('#adminUserErrorMessage').hide();
        }
        else {
            $('#adminUserErrorMessage').text(message);
            $('#adminUserErrorMessage').show();
        }
    }

    function GetPermissionText(permission) {
        switch (permission) {
            case 1: return "Admin";
            case 2: return "Super Admin";
        }
    }

    function GetIndexFromAdmins(admins, searchAdminId) {
        for (var i = 0; i < admins.length; i++) {
            if (admins[i].id === searchAdminId) {
                return i;
            }
        }
    }

    function GetIndexFromLocations(locations, searchLocation) {
        for (var i = 0; i < locations.length; i++) {
            if (locations[i].name === searchLocation) {
                return i;
            }
        }
    }

})();