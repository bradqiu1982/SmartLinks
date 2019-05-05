var DMR = function () {

    var show = function () {

        $('.date').datepicker({ autoclose: true, pickerPosition: "bottom-left", changeMonth: true,changeYear: true });

        var dmrdisttable = null;
        var dmroatable = null;
        var dmrsntable = null;
        var dmrydtable = null;

        var snworkflowtable = null;
        var failist = null;

        var loadprodline = function () {
            $.post('/SmartLinks/LoadDRMProLine', {
            }, function (output) {

                $('#proline').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.prodlist;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    }
                });
                $('#proline').attr('readonly', false);
            })
        }

        $(function () {
            loadprodline();
        });

        var showtable = function (output) {

            //sn distribution
            if (dmrdisttable) {
                dmrdisttable.destroy();
                dmrdisttable = null;
            }
            $("#dmrdisthead").empty();
            $("#dmrdistcontent").empty();

            $("#dmrdisthead").append('<tr>' +
                    '<th>WORK FLOW</th>' +
                    '<th>WORK FLOW STEP</th>' +
                    '<th>MODULE COUNT</th>' +
                    '</tr>');


            $.each(output.moduledist, function (i, val) {
                var appendstr = '<tr>';
                appendstr += '<td>' + val.WorkFlow + '</td>';
                appendstr += '<td>' + val.WorkFlowStep + '</td>';
                appendstr += '<td>' + val.ModuleCount + '</td>';
                appendstr += '</tr>';
                $("#dmrdistcontent").append(appendstr);
            });

            dmrdisttable = $('#dmrdisttable').DataTable({
                'iDisplayLength': 10,
                'aLengthMenu': [[10,20, 50, 100, -1],
                [10,20, 50, 100, "All"]],
                "columnDefs": [
                    { "className": "dt-center", "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });


            //OA Status
            if (dmroatable) {
                dmroatable.destroy();
                dmroatable = null;
            }
            $("#dmroahead").empty();
            $("#dmroacontent").empty();

            $("#dmroahead").append('<tr>' +
                    '<th>DMR</th>' +
                    '<th>DMR DATE</th>' +
                    '<th>OA STEP</th>' +
                    '<th>OA STATUS</th>' +
                    '<th>MODULE COUNT</th>' +
                    '</tr>');


            $.each(output.dmrstatuslist, function (i, val) {
                var appendstr = '<tr>';
                appendstr += '<td>' + val.DMRID + '</td>';
                appendstr += '<td>' + val.DMRDate + '</td>';
                appendstr += '<td>' + val.DMROAStep + '</td>';
                appendstr += '<td>' + val.DMROAStatus + '</td>';
                appendstr += '<td>' + val.ModuleCount + '</td>';
                appendstr += '</tr>';
                $("#dmroacontent").append(appendstr);
            });

            dmroatable = $('#dmroatable').DataTable({
                'iDisplayLength': 10,
                'aLengthMenu': [[10,20, 50, 100, -1],
                [10,20, 50, 100, "All"]],
                "columnDefs": [
                    { "className": "dt-center", "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            //module yield
            if (dmrydtable) {
                dmrydtable.destroy();
                dmrydtable = null;
            }
            $("#dmrydhead").empty();
            $("#dmrydcontent").empty();

            var apstr = '<tr>';
            $.each(output.yielddata.titlelist, function (i, val) {
                apstr += '<th>' + val + '</th>';
            });
            apstr += '</tr>';
            $("#dmrydhead").append(apstr);

            apstr = '<tr>';
            $.each(output.yielddata.passlist, function (i, val) {
                apstr += '<td>' + val + '</td>';
            });
            apstr += '</tr>';
            $("#dmrydcontent").append(apstr);

            apstr = '<tr>';
            $.each(output.yielddata.totlelist, function (i, val) {
                apstr += '<td>' + val + '</td>';
            });
            apstr += '</tr>';
            $("#dmrydcontent").append(apstr);

            apstr = '<tr>';
            $.each(output.yielddata.ydlist, function (i, val) {
                if (val.indexOf('YD:') != -1) {
                    val = val.replace('YD:', '');
                    apstr += '<td class="DMRFAILYDDATA" myid="' + output.yielddata.titlelist[i] + '">' + val + '</td>';
                }
                else {
                    apstr += '<td>' + val + '</td>';
                }
            });
            apstr += '</tr>';
            $("#dmrydcontent").append(apstr);

            dmrydtable = $('#dmrydtable').DataTable({
                'iDisplayLength': 10,
                'aLengthMenu': [[10, 20, 50, 100, -1],
                [10, 20, 50, 100, "All"]],
                "columnDefs": [
                    { "className": "dt-center", "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });


            //sn trace
            if (dmrsntable) {
                dmrsntable.destroy();
                dmrsntable = null;
            }
            $("#dmrsnhead").empty();
            $("#dmrsncontent").empty();

            $("#dmrsnhead").append(
                '<tr style="font-size:12px;">' +
                    '<th>SN</th>' +
                    '<th>SN STAT</th>' +
                    '<th>DMR#</th>' +
                    '<th>OA STEP</th>' +
                    '<th>OA STATUS</th>' +
                    '<th>DMR START</th>' +
                    '<th>DMR RETURN</th>' +
                    '<th>FAILURE</th>' +
                    '<th>CRT WFSTEP</th>' +
                    '<th>CRT WF</th>' +
                    '<th>RW FROM</th>' +
                    '<th>OA(D)</th>' +
                    '<th>DMR STORE(D)</th>' +
                    '<th>DMR REPAIR(D)</th>' +
                    '<th>DMR TOTAL(D)</th>' +
                '</tr>'
                );

            $.each(output.dmrdata, function (i, val) {
                var appendstr = '<tr style="font-size:10px;">';
                appendstr += '<td class="SNWORKFLOW" myid="' + val.SN + '"><strong>' + val.SN + '</strong></td>';
                appendstr += '<td>' + val.SNStatus + '</td>';
                appendstr += '<td>' + val.DMRID + '</td>';
                appendstr += '<td>' + val.DMROAStep + '</td>';
                appendstr += '<td>' + val.DMROAStatus + '</td>';
                appendstr += '<td>' + val.DMRDate + '</td>';
                appendstr += '<td>' + val.DMRReturnTime + '</td>';
                appendstr += '<td>' + val.SNFailure + '</td>';
                appendstr += '<td>' + val.WorkFlowStep + '</td>';
                appendstr += '<td>' + val.WorkFlow + '</td>';
                appendstr += '<td>' + val.DMRRepairStep + '</td>';
                appendstr += '<td>' + val.OASpend + '</td>';
                appendstr += '<td>' + val.StoreSpend + '</td>';
                appendstr += '<td>' + val.RepairSpend + '</td>';
                appendstr += '<td>' + val.TotleDRMSpend + '</td>';
                appendstr += '</tr>';
                $("#dmrsncontent").append(appendstr);
            });


            dmrsntable = $('#dmrsntable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });
        }


        var DMRWIPFUN = function () {
            var prodline = $('#proline').val();
            if (prodline == '')
            {
                alert('Product Line  need to be input for WIP function!');
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/DMRWIPData',
                { prodline: prodline },
                function (output) {
                    $.bootstrapLoading.end();
                    showtable(output);

                    failist = new Array();
                    $.each(output.yielddata.faillist, function (i, val) {
                        failist.push(val);
                    });
            })

        }

        $('body').on('click', '#btn-wip', function () {
            DMRWIPFUN();
        })


        var DMRTRACE = function () {
            var prodline = $('#proline').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (prodline == '' || sdate == '' || edate == '')
            {
                alert('Product Line, start date, end date need to be input for TRACE function!');
                return false;
            }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SmartLinks/DMRTRACEData',
                {
                    prodline: prodline,
                    sdate: sdate,
                    edate: edate
                },
                function (output) {
                    $.bootstrapLoading.end();
                    showtable(output);

                    failist = new Array();
                    $.each(output.yielddata.faillist, function (i, val) {
                        failist.push(val);
                    });
                })
        }

        $('body').on('click', '#btn-trace', function () {
            DMRTRACE();
        })



        $('body').on('click', '.SNWORKFLOW', function () {
            var sn = $(this).attr('myid');
            $.post('/SmartLinks/SNWholeWorkFlow', {
                sn: sn
            },
            function (output) {
                if (snworkflowtable) {
                    snworkflowtable.destroy();
                    snworkflowtable = null;
                }
                $('#snworkflowhead').empty();
                $("#snworkflowcontent").empty();

                $('#snworkflowhead').append(
                    '<tr>'+
                        '<th>SN</th>'+
                        '<th>PN</th>'+
                        '<th>WORKFLOW</th>'+
                        '<th>WORKFLOW STEP</th>'+
                        '<th>JO</th>'+
                        '<th>TIMESTAMP</th>'+
                    '</tr>'
                    );

                $.each(output.snworkflowlist, function (i, val) {
                    var appendstr = '<tr>';
                    appendstr += '<td>' + val.SN + '</td>';
                    appendstr += '<td>' + val.PN + '</td>';
                    appendstr += '<td>' + val.WorkFlow + '</td>';
                    appendstr += '<td>' + val.WorkFlowStep + '</td>';
                    appendstr += '<td>' + val.JO + '</td>';
                    appendstr += '<td>' + val.DMRDate + '</td>';
                    appendstr += '</tr>';
                    $("#snworkflowcontent").append(appendstr);
                });

                snworkflowtable = $('#snworkflowtable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('#snworkflowmodal').modal('show');
            });
        })

        $('body').on('click', '.DMRFAILYDDATA', function () {
            var whichtest = $(this).attr('myid');

            if (failist == null)
            { return false;}

            if (snworkflowtable) {
                snworkflowtable.destroy();
                snworkflowtable = null;
            }
            $('#snworkflowhead').empty();
            $("#snworkflowcontent").empty();

            $('#snworkflowhead').append(
                '<tr>' +
                    '<th>SN</th>' +
                    '<th>WhichTest</th>' +
                    '<th>Failure</th>' +
                    '<th>TIMESTAMP</th>' +
                '</tr>'
                );

            $.each(failist, function (i, val) {
                if (val.WhichTest == whichtest)
                {
                    var appendstr = '<tr>';
                    appendstr += '<td>' + val.SN + '</td>';
                    appendstr += '<td>' + val.WhichTest + '</td>';
                    appendstr += '<td>' + val.Failure + '</td>';
                    appendstr += '<td>' + val.TestTime + '</td>';
                    appendstr += '</tr>';
                    $("#snworkflowcontent").append(appendstr);
                }

            });

            snworkflowtable = $('#snworkflowtable').DataTable({
                'iDisplayLength': 50,
                'aLengthMenu': [[20, 50, 100, -1],
                [20, 50, 100, "All"]],
                "columnDefs": [
                    { "className": "dt-center", "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5', 'csv', 'excelHtml5']
            });

            $('#snworkflowmodal').modal('show');
        });

    }


    return {
        init: function () {
            show();
        }
    }
}();