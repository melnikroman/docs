
function getUsersBirthdays(logins, Success, Error) {
    var clientContext = new SP.ClientContext.get_current();
    var web = clientContext.get_web();
    clientContext.executeQueryAsync(
    function () {
        var peopleManager = new SP.UserProfiles.PeopleManager(clientContext);
        var birthdays = [];
        var FNames = [];
        var LNames = [];
        var PicUrls = [];

        var departments = [];
        var positions = [];

        for (var i = 0; i < logins.length; i++) {
            var personBirthday = peopleManager.getUserProfilePropertyFor(logins[i], 'SPS-Birthday');
            var fname = peopleManager.getUserProfilePropertyFor(logins[i], 'FirstName');
            var lname = peopleManager.getUserProfilePropertyFor(logins[i], 'LastName');
            var department = peopleManager.getUserProfilePropertyFor(logins[i], 'Department');
            var position = peopleManager.getUserProfilePropertyFor(logins[i], 'Title');
            var pic = peopleManager.getUserProfilePropertyFor(logins[i], 'PictureURL');
            birthdays.push(personBirthday);
            FNames.push(fname);
            LNames.push(lname);
            departments.push(department);
            positions.push(position);
            PicUrls.push(pic);
        }

        clientContext.executeQueryAsync(
            function () {
                Success(birthdays, FNames, LNames, departments, positions, PicUrls);
            },
            Error);
    },
    Error);
}


function showUser(logins) {
    var scriptbase = _spPageContextInfo.webAbsoluteUrl + '/_layouts/15/';
    $.getScript(scriptbase + 'SP.js', function () {
        $.getScript(scriptbase + 'SP.UserProfiles.js', function () {
            getUsersBirthdays(logins, function (birthdays, fnames, lnames, departments, positions, pics) {
                var date = new Date();

                var today = new Date((date.getMonth() + 1) + '.' + date.getDate() + '.2000');
                var tomorrow = new Date((date.getMonth() + 1) + '.' + (date.getDate() + 1) + '.2000');
                var yesterday = new Date((date.getMonth() + 1) + '.' + (date.getDate() - 1) + '.2000');
                var twodays = new Date((date.getMonth() + 1) + '.' + (date.getDate() - 2) + '.2000');
                var threedays = new Date((date.getMonth() + 1) + '.' + (date.getDate() - 3) + '.2000');
                var counter = 0;
                for (var i = 0; i < birthdays.length; i++) {
                    var birthdate = new Date(birthdays[i].get_value().split('.')[1] + '.' + birthdays[i].get_value().split('.')[0] + '.2000');
                    //if ((birthdate.valueOf()==today.valueOf())||(birthdate.valueOf()==tomorrow.valueOf())||(birthdate.valueOf()==yesterday.valueOf())){
                    counter += 1;
                    var Day;
                    if (birthdate.valueOf() == today.valueOf()) {
                        Day = 'Сегодня';
                    }
                    if (birthdate.valueOf() == tomorrow.valueOf()) {
                        Day = 'Завтра';
                    }
                    if (birthdate.valueOf() == yesterday.valueOf()) {
                        Day = 'Вчера';
                    }
                    if (birthdate.valueOf() == twodays.valueOf()) {
                        Day = 'Позавчера';
                    }
                    if (birthdate.valueOf() == threedays.valueOf()) {
                        Day = '2 дня назад';
                    }
                    $('.m-informer-birthdays__list-item').append('<div class="m-informer-birthday-item" id="birthday-item' + counter + '"></div>');
                    $('#birthday-item' + counter).append('<div class="thumb" style="background-image: url(' + pics[i].get_value() + ');">thumb</div>');
                    $('#birthday-item' + counter).append('<div class="body" id="body' + counter + '"></div>');
                    $('#body' + counter).append('<div class="date is-today">' + Day + '</div>');
                    $('#body' + counter).append('<div class="employee-name"><div class="last-name">' + lnames[i].get_value() + '</div>' + fnames[i].get_value() + '</div>');
                    $('#body' + counter).append('<div class="employee-post">' + positions[i].get_value() + ' ' + departments[i].get_value() + '</div>');
                    //}


                }
                if (counter == 0) {
                    //$('#resultsDiv').text('День рождений нет.')
                }
            },
            function (sender, args) {
                console.log(args.get_message());
            });
        });
    });
}


Date.prototype.AddDays = function (days) {
    this.setDate(this.getDate() + days);
    return this;
}

function executeQuery() {

    Results = {
        element: '',
        url: '',

        init: function (element) {
            Results.element = element;

            var birthday = 'Birthday';
            var space = '%20';
            var colon = '%3A';
            var quote = '%22';
            var gt = '%3E';
            var lt = '%3C';
            var amp = '&';

            // Get current date
            var currentTime = new Date();
            var startMonth = currentTime.getMonth() + 1;
            var day = currentTime.getDate();



            var endTime = currentTime.AddDays(1);


            var yesterday = day - 1;
            var twodays = day - 2;
            var threedays = day - 3;
            var endMonth = endTime.getMonth() + 1;
            var endDay = endTime.getDate();

            var querytext = "";

            // build query with the magic 2000 year
            if (startMonth != '12') {
                querytext += birthday + colon + quote + startMonth + '/' + day + '/' + '2000*' + quote + space + 'OR' + space + birthday + colon + quote + endMonth + '/' + endDay + '/' + '2000*' + quote +
                space + 'OR' + space + birthday + colon + quote + endMonth + '/' + yesterday + '/' + '2000*' + quote +
                space + 'OR' + space + birthday + colon + quote + endMonth + '/' + twodays + '/' + '2000*' + quote +
                space + 'OR' + space + birthday + colon + quote + endMonth + '/' + threedays + '/' + '2000*' + quote;
            }
            else {
                querytext += birthday + colon + quote + startMonth + '/' + day + '/' + '2000*' + quote + space + 'OR' + space + birthday + colon + quote + endMonth + '/' + endDay + '/' + '2000*' + quote +
                space + 'OR' + space + birthday + colon + quote + endMonth + '/' + yesterday + '/' + '2000*' + quote;
                //querytext += birthday + gt + quote + day + '-' + startMonth + '-' + '2000' + quote + space + 'AND' + space + birthday + lt + quote + endDay + '-' + endMonth + '-' + '2000' + quote;

            }
            Results.url = _spPageContextInfo.webAbsoluteUrl + '/_api/search/query?querytext=%27' + querytext + '%27&selectproperties=%27AccountName%2CBirthday%27';
        },

        load: function () {
            $.ajax(
                    {
                        url: Results.url,
                        method: "GET",
                        headers: {
                            "accept": "application/json; odata=verbose",
                        },
                        success: Results.onSuccess,
                        error: Results.onError
                    }
                );
        },

        onSuccess: function (data) {
            var html = "";
            var results;
            try {
                results = data.d.query.PrimaryQueryResult.RelevantResults;
            }
            catch (e) {
                html += "Ошибка поиска";
                return;
            }

            var logins = [];
            for (var i = 0; i < results.TotalRows; i++) {

                var result = results.Table.Rows.results[i].Cells;
                for (var k = 0; k < result.results.length; k++) {
                    if (result.results[k].Key == 'AccountName') {
                        logins.push(result.results[k].Value);
                    }
                }
            }

            showUser(logins);

            //if (results.length == 0) {
            //    html += "День рождений сегодня нет";
            //}
            Results.element.html(html);
        },

        onError: function (err) {
            //Results.element.html(JSON.stringify(err));
        }
    }

    Results.init($('#resultsDiv'));
    Results.load();

}


