var Asset = function () {
    var show = function () {
        $('[data-toggle="popover"]').popover({ html: true});
        $('body').on('click', '#btn-clear', function () {
            $('.search-filter').find('input[type=text]').val('');
            $('#asset-status').val('');
            window.location.href = '/Asset/Index';
        })

        $('body').on('click', '#btn-search', function () {
            var cn = $.trim($('#s-cn').val());
            var name = $.trim($('#s-name').val());
            var status = $.trim($('#s-status').val());
            window.location.href = '/Asset/Index?cn=' + cn + '&name=' + name + '&status=' + status;
        })

        $('body').on('click', '.pages', function () {
            var page = $(this).attr("data-data");
            window.location.href = '/Asset/Index?p=' + page;
        })

        $('body').on('change', '#upload', function () {
            ajaxFileUploadx();
        })

        $('body').on('click', '.asset-check', function () {
            var id = $(this).data('id');
            $('#mc-comment').val("");
            $('#mc-id').val(id);
            $('#modal-check').modal('show');
        })

        $('body').on('click', '#btn-mclear', function () {
            $('#modal-check').modal('hide');
        })

        $('body').on('click', '#btn-msubmit', function () {
            var assetid = $('#mc-id').val();
            var comment = $.trim($('#mc-comment').val());
            $.post('/Asset/AddCheck',
            {
                assetid: assetid,
                comment: comment
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("This equipment has been in calibration");
                }
            })
        })

        $('body').on('click', '.check-complete', function () {
            var id = $(this).data('id');
            $.post('/Asset/UpdateCheck', {
                id: id
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            })
        })

        function ajaxFileUploadx() {
            var files = $('#upload')[0].files[0];
            var filename = files.name;
            var filetype = filename.split('.')[filename.split('.').length - 1];
            var exts = ["xls", "xlsx"];
            if ($.inArray(filetype, exts) == -1) {
                alert("." + filetype + " is not allowed !");
                return false;
            }
            $.ajaxFileUpload({
                url: '/userfiles/Upload.ashx',
                secureuri: false,
                fileElementId: 'upload',
                dataType: 'json',
                success: function (data) {
                    if (data.url == "") {
                        return false;
                    }
                    $.post('/Asset/UploadAsset',
                    {
                        path: data.url
                    }, function () {
                        window.location.reload();
                    })
                },
                error: function (e) {
                    alert(e);
                }
            })
            return false;
        }
    }

    var myasset = function () {
        $('body').on('click', '.pages', function () {
            var pageno = $(this).attr('data-data');
            window.location.href = '/Asset/MyAsset?p=' + pageno;
        })

        $('body').on('click', '#btn-search', function () {
            searchfunc();
        })

        $('body').on('keypress', '#searchkey', function (e) {
            if (e.keyCode == 13) {
                searchfunc();
            }
        })

        function searchfunc() {
            var keywords = $.trim($('#searchkey').val());
            if (keywords) {
                $.post('/Asset/SearchMyAsset',
                {
                    keywords: keywords
                }, function (output) {
                    if (output.success) {
                        alert(output.data);
                    }
                })
            }
        }
    }

    var borrow = function () {
        $('.date').datepicker({ autoclose: true});
        $('[data-toggle="popover"]').popover({ html: true });
        $.post('/Asset/GetAutoCompleteData',
        {
        }, function (output) {
            autoCompleteFill('m-asset-key', output.assets);
            autoCompleteFill1('m-b-user', output.users);
            autoCompleteFill1('m-pro-no', output.projects);
        })
        $('body').on('click', '.add-icon', function () {
            $('#modal-borrow').find('input[type=text]').val('');
            $('#modal-borrow').find('input[type=radio]').prop("checked", false);
            $('#m-comment').val("");
            $('.m-assets').remove();
            $('#modal-borrow').modal('show');
        })

        $('body').on('click', '.del-m-asset', function () {
            $(this).parent().remove();
        })

        $('body').on('click', '#btn-clear', function () {
            $('#modal-borrow').modal('hide');
        })

        $('body').on('click', '#btn-submit', function () {
            //get data
            var borrow_user = $.trim($('#m-b-user').val());
            var pro_no = $.trim($('#m-pro-no').val());
            var islong = $('input[name=m-islong]:checked').val();
            var sdate = $('#m-sdate').val();
            var edate = $('#m-edate').val();
            var comment = $.trim($('#m-comment').val());
            if (borrow_user == "") {
                alert("Please select borrower!");
                return false;
            }
            if (pro_no == "") {
                alert("Please select project!");
                return false;
            }
            if (islong == undefined) {
                alert("Please choose islong !");
                return false;
            }

            var assets = new Array();
            $('.m-assets').each(function () {
                assets.push($(this).attr('data-val'));
            });
            if (assets.length == 0) {
                alert("Please select one asset at least!");
                return false;
            }
            $.post('/Asset/AddBorrowRequest', {
                borrow_user: borrow_user,
                pro_no: pro_no,
                islong: islong,
                sdate: sdate,
                edate: edate,
                comment: comment,
                assets: JSON.stringify(assets)
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            })
        })

        $('body').on('click', '.img-return', function () {
            var id = $(this).attr('data-id');
            var assetid = $(this).attr('data-assetid');
            var assetInfo = $(this).parent().prev().html();
            $('#mr-id').val(id);
            $('#mr-assetid').val(assetid);
            $('#mr-asset').html(assetInfo);
            $('#mr-comment').val("");
            $('#modal-return').modal('show');
        })

        $('body').on('click', '#btn-return', function () {
            var id = $('#mr-id').val();
            var assetid = $('#mr-assetid').val();
            var comment = $('#mr-comment').val();
            $.post('/Asset/BorrowReturn', {
                id: id,
                assetid: assetid,
                comment: comment
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            });
        })

        $('body').on('click', '#btn-rclear', function () {
            $('#modal-return').modal('hide');
        })

        $('body').on('click', '.pages', function () {
            var page = $(this).attr("data-data");
            var keywords = $.trim($('#searchkey').val());
            window.location.href = '/Asset/BorrowRequest?p=' + page + "&key=" + keywords;
        })

        $('body').on('click', '#btn-search', function () {
            var keywords = $.trim($('#searchkey').val());
            window.location.href = "/Asset/BorrowRequest?p=1&key=" + keywords;
        })

        $('body').on('keypress', '#searchkey', function (e) {
            if (e.keyCode == 13) {
                var keywords = $.trim($('#searchkey').val());
                window.location.href = "/Asset/BorrowRequest?p=1&key=" + keywords;
            }
        })

        $('body').on('click', '.in-borrow', function () {
            window.location.href = "/Asset/BorrowRequest?p=1&status=1";
        })

        function autoCompleteFill(id, values) {
            $('#' + id).autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = values;
                    var suggestions = [];
                    $.each(choices, function (i, val) {
                        if (~(val.CN + ' / ' + val.EnglishName + ' / ' + val.ChineseName).toLowerCase().indexOf(term))
                            suggestions.push(val);
                    })
                    suggest(suggestions);
                },
                renderItem: function (item, search) {
                    search = search.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
                    var re = new RegExp("(" + search.split(' ').join('|') + ")", "gi");
                    return '<div class="autocomplete-suggestion" data-cn="' + item.CN + '" data-id="' + item.ID + '" data-engname="' + item.EnglishName + '" data-chname="' + item.ChineseName + '"> ' + (item.CN + ' / ' + item.EnglishName + ' / ' + item.ChineseName).replace(re, "<b>$1</b>") + '</div>';
                },
                onSelect: function (e, term, item) {
                    var flg = false;
                    $('.m-assets').each(function () {
                        if ($(this).attr('data-val') == item.data('id')) {
                            flg = true; 
                            return;
                        }
                    });
                    if (!flg) {
                        $('.m-add-list').append(
                            '<div class="m-assets" data-val="' + item.data('id') + '">' +
                            '<span class="asset-no-list">' + item.data('engname') + '</span>' +
                            '<span class="glyphicon glyphicon-remove del-m-asset"></span>' +
                            '</div >'
                        );
                    }
                }
            });
        }

        function autoCompleteFill1(id, values) {
            $('#' + id).autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = values;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
        }
    }

    var buy = function () {
        $('[data-toggle="popover"]').popover({ html: true, trigger: "hover"});
        $('body').on('click', '.add-icon', function () {
            $('input[type=text]').val("");
            $('textarea').val("");
            $('#modal-buy').modal('show');
        })

        $('body').on('click', '.check-all', function () {
            if ($(this).prop('checked')) {
                $('.check-item').prop('checked', true);
            }
            else {
                $('.check-item').prop('checked', false);
            }
        })

        $('body').on('click', '.check-item', function () {
            var flg = true;
            if ($(this).prop('checked')) {
                $('.check-item').each(function () {
                    if ( ! $(this).prop('checked')) {
                        flg = false;
                        return;
                    }
                })
            }
            else {
                flg = false;
            }
            $('.check-all').prop('checked', flg);
        })

        $('body').on('click', '#btn-op-update', function () {
            var buy_ids = new Array();
            $('.check-item:checked').each(function () {
                buy_ids.push($(this).attr('data-id'));
            });
            if (buy_ids.length == 0) {
                alert("Please select one device at least");
                return false;
            }
            $.post('/Asset/UpdateBuyRequest',
            {
                buy_ids: buy_ids
            }, function (output) {
                if (output.success) {
                    window.location.reload(); 
                }
            })
        })

        $('body').on('click', '#btn-op-del', function () {
            var del_ids = new Array();
            $('.check-item:checked').each(function () {
                del_ids.push($(this).attr('data-id'));
            });
            if (del_ids.length == 0) {
                alert("Please select one device at least");
                return false;
            }
            if (!confirm("Realy to delete these buy requests?")) {
                return false;
            }
            $.post('/Asset/DeleteBuyRequest',
            {
                del_ids: del_ids
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            })
        })

        $('body').on('click', '#btn-search', function () {
            var keywords = $.trim($('#searchkey').val());
            window.location.href = '/Asset/BuyRequest?p=1&key=' + keywords;
        })

        $('body').on('keypress', '#searchkey', function (e) {
            if (e.keyCode == 13) {
                var keywords = $.trim($('#searchkey').val());
                window.location.href = '/Asset/BuyRequest?p=1&key=' + keywords;
            }
        })

        $('body').on('click', '.pages', function () {
            var page = $(this).attr("data-data");
            var keywords = $.trim($('#searchkey').val());
            window.location.href = '/Asset/BuyRequest?p=' + page + '&key=' + keywords;
        })

        $('body').on('click', '#btn-clear', function () {
            $('#modal-buy').modal('hide');
        })

        $('body').on('click', '#btn-submit', function () {
            var id = $.trim($('#m-id').val());
            var ename = $.trim($('#m-ename').val());
            var cname = $.trim($('#m-cname').val());
            var price = $.trim($('#m-price').val());
            var brand = $.trim($('#m-brand').val());
            var model = $.trim($('#m-model').val());
            var country = $.trim($('#m-country').val());
            var purpose = $.trim($('#m-purpose').val());
            var funcs = $.trim($('#m-funcs').val());
            var principle = $.trim($('#m-principle').val());
            var corpurpose = $.trim($('#m-corpurpose').val());
            var fd = new FormData();
            fd.append('id', id);
            fd.append('ename', ename);
            fd.append('cname', cname);
            fd.append('price', price);
            fd.append('brand', brand);
            fd.append('model', brand);
            fd.append('country', country);
            fd.append('purpose', purpose);
            fd.append('funcs', funcs);
            fd.append('principle', principle);
            fd.append('corpurpose', corpurpose);
            if (document.getElementById("m-picture").files.length > 0) {
                fd.append("picture", document.getElementById("m-picture").files[0]);
            }
            else {
                fd.append("picture", "");
            }

            $.ajax({
                type: 'POST',
                url: '/Asset/AddBuyRequest',
                data: fd,
                processData: false,
                contentType: false
            }).done(function (output) {
                if (output.success) {
                    window.location.reload();
                }
            });
        })

        $('body').on('click', '.buy-del', function () {
            if (!confirm("Realy to delete this buy request?")) {
                return false;
            }
            var del_id = $(this).data('id');
            var del_ids = new Array();
            del_ids.push(del_id);
            $.post('/Asset/DeleteBuyRequest',
            {
                del_ids: del_ids
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            })
        })

        $('body').on('click', '.buy-edit', function () {
            var id = $(this).data('id');
            $.post('/Asset/GetBuyInfo',
            {
                id: id
            }, function (output) {
                if (output.success) {
                    $('#m-id').val(output.data.ID);
                    $('#m-ename').val(output.data.EngName);
                    $('#m-cname').val(output.data.ChName);
                    $('#m-price').val(output.data.UnitPrice);
                    $('#m-brand').val(output.data.Brand);
                    $('#m-model').val(output.data.Model);
                    $('#m-country').val(output.data.OriginCountry);
                    $('#m-purpose').val(output.data.Purpose);
                    $('#m-funcs').val(output.data.Functions);
                    $('#m-principle').val(output.data.WorkPrinciple);
                    $('#m-corpurpose').val(output.data.CorporatePurposes);
                    $('#modal-buy').modal('show');
                }
            })
        })
    }

    return {
        init: function () {
            show();
        },
        myasset: function () {
            myasset();
        },
        borrow: function() {
            borrow();
        },
        buy: function () {
            buy();
        }
    }
}();