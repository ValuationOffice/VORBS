(function () {

    angular.module('vorbs.admin')
        .controller('UsersController', UsersController);

    UsersController.$inject = ['$scope', '$http', '$resource'];

    function UsersController($scope, $http, $resource) {

        CreateUserAdminServices($resource);

        $scope.Locations = Locations.query({});
        $scope.admins = Admins.getAll({});
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

            //Dont need to validate as we get detials from Active Directory. AD Data is filtered on server first.

            ////Validate First & Last Name
            //if ($scope.adminUser.FirstName === "" || $scope.adminUser.LastName === "") {
            //    SetAdminErrorMessage('Invalid First/Last Name.');
            //    return;
            //}

            ////Validate Email
            //if (!ValidateEmail($scope.adminUser.Email)) {
            //    SetAdminErrorMessage('Invalid Email Detected.');
            //    return;
            //}

            $.ajax({
                type: "POST",
                data: JSON.stringify($scope.adminUser),
                url: "api/admin",
                contentType: "application/json",
                success: function () {
                    alert('User Added Successfully!');
                    $scope.CreateNewUserObject();
                    $scope.admins = Admins.getAll({});
                },
                error: function (error) {
                    if (error.status == 409) {
                        SetAdminErrorMessage('Administrator already exists.');
                    }
                    else {
                        alert('Unable to Add Admin. Please Contact ITSD. ' + error.message);
                    }
                }
            });

            SetAdminErrorMessage('');
        }

        $scope.GetAdmin = function (id) {
            $scope.SetAdminId(id);

            var indexAdmin = GetIndexFromAdmins($scope.admins, $scope.adminId);
            if (indexAdmin >= 0) {
                $scope.editAdmin = $scope.admins[indexAdmin];
                $scope.editAdminUser.permissionLevel = $scope.editAdmin.permissionLevel
                $scope.editAdminUser.location.id = $scope.editAdmin.location.id
            }
            else {
                alert('Unable to retrieve admin data. Admin may have already been deleted.');
                $('#editAdminModal').modal('hide');
            }
        }

        $scope.EditAdmin = function (id) {
            var dateChanged = false;
            //Change the "edit booking" button to stop multiple edits
            $("#editAdminEditButton").prop('disabled', 'disabled');
            $("#editAdminEditButton").html('Editing Admin. Please wait..');

            if ($scope.editAdminUser.location.id !== $scope.editAdmin.LocationID) {
                $scope.editAdmin.LocationID = $scope.editAdminUser.location.id
                dateChanged = true;
            }

            if ($scope.editAdminUser.permissionLevel != $scope.editAdmin.permissionLevel) {
                $scope.editAdmin.permissionLevel = $scope.editAdminUser.permissionLevel;
                dateChanged = true;
            }

            //im sorry, in a rush and we dont need more locations!!
            delete $scope.editAdmin.location

            if (!dateChanged) {
                alert('Admin data has not changed.');
                EnableEditAdminButton();
                return;
            }

            try {
                $.ajax({
                    type: "POST",
                    data: JSON.stringify($scope.editAdmin),
                    url: "api/admin/" + $scope.adminId,
                    contentType: "application/json",
                    success: function (data, status) {
                        alert('Admin Edited Successfully!');
                        $scope.admins = Admins.getAll({});
                        $('#editAdminModal').modal('hide');
                    },
                    error: function (error) {
                        alert('Unable to edit the admin. Please try again or contact ITSD. ' + error.message);
                    }
                });
            } catch (e) {
                EnableEditAdminButton();
            }

            EnableEditAdminButton();
        }

        $scope.DeleteAdmin = function () {
            Admins.removeAdminById({
                adminId: $scope.adminId
            },
                function (success) {
                    $scope.admins = Admins.getAll({});
                    $('#deleteAdminModal').modal('hide');
                },
                function (error) {
                    alert('Unable to delete the admin. Please try again or contact ITSD. ' + error.message); //TODO:Log Error
                })
        };

        $('#activeDirecotryModal').on('show.bs.modal', function () {
            $scope.GetAdNames();
        });

        $scope.GetAdNames = function () {
            if ($scope.emailVal === undefined || $scope.emailVal.trim() === "") {
                $scope.adAdminUsers = AdUsers.queryAll({
                    allUsers: true
                })
            }
            else {
                $scope.adAdminUsers = AdUsers.querySurname({
                    allUsers: $scope.emailVal
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

        $scope.editAdminUser = {
            location: {
                id: ''
            },
            permissionLevel: 1
        }

        $scope.adAdminUser = {
            pID: '',
            firstName: '',
            lastName: '',
            email: ''
        };
    }

    function CreateUserAdminServices($resource) {
        Admins = $resource('/api/admin/:adminId', { adminId: 'adminId' },
            {
                getAll: { method: 'GET', isArray: true },
                getAdminById: { method: 'GET' },
                removeAdminById: { method: 'DELETE' }
            });

        Admins.prototype = {
            permissionLevelText: function () { return GetPermissionText(this.permissionLevel); }
        };

        Locations = $resource('/api/locations', {}, {
            query: { method: 'GET', isArray: true }
        });

        AdUsers = $resource('/api/users/:allUsers', { allUsers: 'allUsers' },
            {
                queryAll: { method: 'GET', isArray: true },
                querySurname: { method: 'GET', isArray: true }
            });
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