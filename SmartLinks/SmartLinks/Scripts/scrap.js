var Scrap = function () {
    var mainpage = function () {
        var myscraptable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshscrapTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("查询条件不可为空！");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);
            $.post('/ScrapHelper/QueryScrap',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               var idx = 0;
               var datacont = output.data.length;

               if (myscraptable) {
                   myscraptable.destroy();
               }
               $("#ScrapTableID").empty();

               for (idx = 0; idx < datacont; idx++) {
                   var line = output.data[idx];
                   if (line.Result === "直接报废") {
                       $("#ScrapTableID").append('<tr class="tr-danger"><td>' + line.SN + '</td><td>' + line.DateCode + '</td><td>'
                       + line.PN + '</td><td>' + line.WhichTest + '</td><td>' + line.TestData.ErrAbbr
                       + '</td><td>' + line.MatchedRule + '</td><td>' + line.Result + '</td></tr>');
                   }
                   else if (line.Result === "隔离报废")
                   {
                       $("#ScrapTableID").append('<tr class="tr-warning"><td>' + line.SN + '</td><td>' + line.DateCode + '</td><td>'
                      + line.PN + '</td><td>' + line.WhichTest + '</td><td>' + line.TestData.ErrAbbr
                      + '</td><td>' + line.MatchedRule + '</td><td>' + line.Result + '</td></tr>');
                   }
                   else {
                       $("#ScrapTableID").append('<tr class="tr-success"><td>' + line.SN + '</td><td>' + line.DateCode + '</td><td>'
                       + line.PN + '</td><td>' + line.WhichTest + '</td><td>' + line.TestData.ErrAbbr
                       + '</td><td>' + line.MatchedRule + '</td><td>' + line.Result + '</td></tr>');
                   }
               }
               $.bootstrapLoading.end();
               myscraptable = $('#myscraptable').DataTable({
                   'iDisplayLength': 50,
                   'aLengthMenu': [[20, 50, 100, -1],
                   [20, 50, 100, "All"]],
                   "aaSorting": [],
                   "order": []
               });

           })
        }

        
        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
        })

        $('body').on('click', '#btn-marks-submit', function () {
            RefreshscrapTable(true);
        })

        $('body').on('click', '.op-setting', function () {
            window.location.href = "/ScrapHelper/SettingScrapRule";
        })

        $('body').on('click', '.op-download', function () {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                alert("查询条件不可为空！");
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            $.post('/ScrapHelper/DownloadScrapData',
            {
                marks: JSON.stringify(cur_marks)
            }, function (output) {
                window.open(output.data, '_blank');
            })
        })

        $('body').on('click', '.op-historydownload', function () {
            $.post('/ScrapHelper/DownloadScrapHistory',
            {
            }, function (output) {
                window.open(output.data, '_blank');
            })
        })

    }

    var settingpage = function () {
        $(function () {
            stationfill();
            errorabbrfill();
            testcasefill();
        });

        $('body').on('mouseenter', '.pn-del', function () {
            $(this).children().eq(0).removeClass('hidden');
        })
        $('body').on('mouseleave', '.pn-del', function () {
            $(this).children().eq(0).addClass('hidden');
        })
        $('body').on('click', '.pn-del', function () {
            if (confirm("Confirm to delete this pn! ")) {
                var pnkey = $(this).attr('data-pnkey');
                $.post('/ScrapHelper/RemovePN', {
                    pnkey: pnkey
                },
                function (output) {
                    window.location.href = '/ScrapHelper/SettingScrapRule';
                });
            }
        })
        $('body').on('click', '.item-show', function () {
            var activepn = $(this).attr('data-pn');
            window.location.href = '/ScrapHelper/SettingScrapRule?ActivePN=' + activepn;
            //$('li').removeClass('active');
            //$(this).parent().parent().addClass('active');
        })

        $('body').on('click', '#btn-pn-search', function () {
            var searchkey = $('#pn-search').val();
            window.location.href = '/ScrapHelper/SettingScrapRule?ActivePN=&SearchKey=' + searchkey;
            //$('li').removeClass('active');
            //$(this).parent().parent().addClass('active');
        })

        $('body').on('click', '.ReturnMainPage', function () {
            window.location.href = '/ScrapHelper/Index';
        })

        $('body').on('click', '.pn-add', function () {
            $('#m-pn-no').val('');
            $('#m-project').val('');
            $('#modal-pn-add').modal('show');
        })
        $('body').on('click', '#m-save-pn', function () {
            var npn = $.trim($('#m-pn-no').val());
            var npro = $.trim($('#m-project').val());
            //var defres = $('#defreslist').val();
            var defres = '';

            if (npn == "") {
                alert("Please input pn num! ");
                return false;
            }

            var duppn = false;
            $('.pn-show').each(function () {
                if ($(this).attr('data-pn') === npn)
                {
                    duppn = true;
                    return false;
                }
            });
            if (duppn) {
                alert("PN already exist! ");
                return false;
            }

            $.post('/ScrapHelper/NewPN', {
                 npn: npn,
                 npro: npro,
                 defres: defres
            }, function (output) {
                window.location.href = '/ScrapHelper/SettingScrapRule?ActivePN='+npn;
            //var appendStr = '<li><a>' +
            //    '<div class="item-show">' + npn + ((npro != "") ? (" -- " + npro) : "") + '</div>' +
            //        '<div class="pn-del">' +
            //            '<span class="glyphicon glyphicon-remove hidden"></span>' +
            //        '</div>' +
            //    '</a></li>';
            //$(appendStr).insertBefore($('.li-pn-add'));
            //$('#modal-pn-add').modal('hide');
             })
        })

        $('body').on('click', '#btn-bind', function () {
            var pnkey = $.trim($('#h-pn-key').val());
            var npn = $.trim($('#pn-no').val());
            var npro = $.trim($('#pro-key').val());
            var defres = $.trim($('#updatedefreslist').val());

            var fd = new FormData();
            if (document.getElementById("con-file").files.length > 0) {
                fd.append("fileToUpload", document.getElementById("con-file").files[0]);
            }
            fd.append("pnkey", pnkey);
            fd.append("npn", npn);
            fd.append("npro", npro);
            fd.append("defres", defres);

            var xhr = new XMLHttpRequest();
            xhr.open("POST", "/ScrapHelper/UpdatePNMainVM", true);
            xhr.addEventListener("load", function (e) {
                if (e.target.status == 200) {
                    $('#showfilename').text('');
                    stationfill();
                }
                else {
                    alert("Error Uploading File");
                }
            }, false);
            xhr.send(fd);

        })

        var stationfill = function () {
            var pnkey = $('#h-pn-key').val();
            $.post('/ScrapHelper/GetPNWhichTables',
                     {
                         pnkey: pnkey
                     }, function (output) {
                         if (output.data.length > 0)
                         {
                            $('.pn-rules-title').removeClass('hide');
                            $('.pn-rules').removeClass('hide');

                            $('#m-whichtest').autoComplete('destroy');
                            $('#m-whichtest').autoComplete({
                                minChars: 0,
                                source: function (term, suggest) {
                                    term = term.toLowerCase();
                                    var suggestions = [];
                                for (i = 0; i < output.data.length; i++) {
                                    if (output.data[i].toLowerCase().indexOf(term) != -1) {
                                        suggestions.push(output.data[i]);
                                    }
                                }
                                suggest(suggestions);
                                }
                            });
                         }
                     });
        }

        var errorabbrfill = function () {
            $.post('/ScrapHelper/GetAllErrAbbr',
                     {},
                     function (output) {
                         if (output.data.length > 0) {

                             $('#m-errabbr').autoComplete('destroy');
                             $('#m-errabbr').autoComplete({
                                 minChars: 0,
                                 source: function (term, suggest) {
                                     term = term.toLowerCase();
                                     var suggestions = [];
                                     for (i = 0; i < output.data.length; i++) {
                                         if (output.data[i].toLowerCase().indexOf(term) != -1) {
                                             suggestions.push(output.data[i]);
                                         }
                                     }
                                     suggest(suggestions);
                                 }
                             });
                         }
                     });
        }

        var testcasefill = function () {
            $.post('/ScrapHelper/GetAllTestCase',
                     {},
                     function (output) {
                         if (output.data.length > 0) {

                             $('#m-testcase').autoComplete('destroy');
                             $('#m-testcase').autoComplete({
                                 minChars: 0,
                                 source: function (term, suggest) {
                                     term = term.toLowerCase();
                                     var suggestions = [];
                                     for (i = 0; i < output.data.length; i++) {
                                         if (output.data[i].toLowerCase().indexOf(term) != -1) {
                                             suggestions.push(output.data[i]);
                                         }
                                     }
                                     suggest(suggestions);
                                 }
                             });
                         }
                     });
        }

        $('body').on('click', '#btn-add-rule', function () {
            $('#m-rule-id').val('');
            $('#m-whichtest').val('');
            $('#m-errabbr').val('');
            $('#m-testcase').val('');
            $('#m-param').val('');
            $('#m-min').val('');
            $('#m-max').val('');
            $('#modal-rule-add').modal('show');
        })

        $('body').on('click', '#m-save-rule', function () {
            var pnkey = $.trim($('#h-pn-key').val());
            var rule_id = $.trim($('#m-rule-id').val());

            var whichtest = $.trim($('#m-whichtest').val());
            var errabbr = $.trim($('#m-errabbr').val());
            var testcase = $.trim($('#m-testcase').val());
            var param = $.trim($('#m-param').val());
            var min = $.trim($('#m-min').val());
            var max = $.trim($('#m-max').val());
            var ruleres = $.trim($('#ruleresultlist').val());

            if (whichtest == "" || errabbr == "")
            {
                alert("Which-Test and ErrAbbr is necessary!");
                return false;
            }

            if (param)
            {
                if (min == "") { min = '-99999'; }
                if (max == "") { max = '99999'; }
            }

            if (rule_id == "") {
                $.post('/ScrapHelper/UpdatePNRule',
                 {
                     rule_id:rule_id,
                     pnkey: pnkey,
                     whichtest: whichtest,
                     errabbr: errabbr,
                     testcase: testcase,
                     param : param,
                     min: min,
                     max: max,
                     ruleres: ruleres
                 }, function(output){
                     if (output.sucess) {
                         var appendStr = '<tr id="' + output.rid + '">'
                                + '<td>' + whichtest + '</td>'
                                + '<td>' + errabbr + '</td>'
                                + '<td>' + testcase + '</td>'
                                + '<td>' + param + '</td>'
                                + '<td>' + min + '</td>'
                                + '<td>' + max + '</td>'
                                + '<td>' + ruleres + '</td>'
                                + '<td>'
                                    + '<a data-id="'+output.rid+'" class="edit-rule">Edit</a>'
                                    + '<a data-id="'+output.rid+'" class="del-rule">Delete</a>'
                                + '</td>'
                            + '</tr>';

                        $('#rules-body').append(appendStr);
                        $('#modal-rule-add').modal('hide');
                     }
                 })
            }
            else {
                $.post('/ScrapHelper/UpdatePNRule',
                 {
                    rule_id:rule_id,
                    pnkey: pnkey,
                    whichtest: whichtest,
                    errabbr: errabbr,
                    testcase: testcase,
                    param : param,
                    min: min,
                    max: max,
                    ruleres: ruleres
                 }, function(output){
                     if (output.sucess) {
                        $('#' + rule_id).children().eq(0).html(whichtest);
                        $('#' + rule_id).children().eq(1).html(errabbr);
                        $('#' + rule_id).children().eq(2).html(testcase);
                        $('#' + rule_id).children().eq(3).html(param);
                        $('#' + rule_id).children().eq(4).html(min);
                        $('#' + rule_id).children().eq(5).html(max);
                        $('#' + rule_id).children().eq(6).html(ruleres);
                        $('#modal-rule-add').modal('hide');
                        $('#m-rule-id').val('');
                     }
                 })
            }
        })

        //$('body').on('click', '#btn-search', function () {
        //    search_func();
        //})

        //$('body').on('keypress', '#tb-search', function (e) {
        //    if (e.keyCode == 13) {
        //        search_func();
        //    }
        //})

        //var search_func = function () {
        //    var keywords = $.trim($('#tb-search').val());
        //    if (keywords == "") {
        //        return false;
        //    }
        //    var pn = $.trim($('#h-pn-no').val());
        //    // $.post('/', {
        //    //     keywords: keywords,
        //    //     pn: pn
        //    // }, function(output){
        //    //     if(output.success){
        //    $('#rules-body').empty();
        //    var appendStr = "";
        //    // $.each(output.data, function(){
        //    appendStr += '<tr>'
        //        + '<td>Initial</td>'
        //        + '<td>LoPwr</td>'
        //        + '<td>15</td>'
        //        + '<td>15.31%</td>'
        //        + '<td>-4</td>'
        //        + '<td>NA</td>'
        //        + '<td>>-4dbm 保留</td>'
        //        + '<td>'
        //            + '<a data-id="data-id" class="edit-rule">Edit</a>'
        //            + '<a data-id="data-id" class="del-rule">Delete</a>'
        //        + '</td>'
        //    + '</tr>';
        //    // })
        //    $('#rules-body').append(appendStr);
        //    //     }
        //    // });
        //}

        $('body').on('click', '.edit-rule', function () {
            var rule_id = $(this).data('id');
            var whichtest = $.trim($('#' + rule_id).children().eq(0).html());
            var errabbr = $.trim($('#' + rule_id).children().eq(1).html());
            var testcase = $.trim($('#' + rule_id).children().eq(2).html());
            var param = $.trim($('#' + rule_id).children().eq(3).html());
            var min = $.trim($('#' + rule_id).children().eq(4).html());
            var max = $.trim($('#' + rule_id).children().eq(5).html());
            var ruleres = $.trim($('#' + rule_id).children().eq(6).html());
            $('#m-rule-id').val(rule_id);
            $('#m-whichtest').val(whichtest);
            $('#m-errabbr').val(errabbr);
            $('#m-testcase').val(testcase);
            $('#m-param').val(param);
            $('#m-min').val(min);
            $('#m-max').val(max);
            $('#ruleresultlist').val(ruleres);
            $('.modal-title').html('Edit Rule');
            $('#modal-rule-add').modal('show');
        })

        $('body').on('click', '.del-rule', function () {
            if (confirm('Confirm to delete?')) {
                var rule_id = $(this).data('id');
                $.post('/ScrapHelper/RemovePNRule',
                 {
                     rule_id: rule_id
                 }, function(output){
                     if(output.sucess){
                $('#' + rule_id).remove();
                     }
                 }); 
            }
        })
    }

    return {
        mainpageinit: function () {
            mainpage();
        },
        settingpageinit: function ()
        {
            settingpage();
        }
    }
}();