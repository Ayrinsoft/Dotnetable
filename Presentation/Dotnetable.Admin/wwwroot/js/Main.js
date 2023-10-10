var theEditor;
let currentState;
let createdEditors = [];
var Plugin = Plugin || {};

class MyUploadAdapter {
    constructor(loader) { this.loader = loader; }
    upload() {
        return this.loader.file
            .then(file => new Promise((resolve, reject) => {
                this._initRequest();
                this._initListeners(resolve, reject, file);
                this._sendRequest(file);
            }));
    }

    abort() {
        if (this.xhr) {
            this.xhr.abort();
        }
    }

    _initRequest() {
        const xhr = this.xhr = new XMLHttpRequest();

        xhr.open('POST', '/api/Files/UploadImage', true);
        xhr.setRequestHeader('Authorization', 'Bearer ' + currentState);
        xhr.responseType = 'json';
    }

    _initListeners(resolve, reject) {
        const xhr = this.xhr;
        const loader = this.loader;
        const genericErrorText = 'Couldn\'t upload file:' + ` ${loader.file.name}.`;

        xhr.addEventListener('error', () => reject(genericErrorText));
        xhr.addEventListener('abort', () => reject());
        xhr.addEventListener('load', () => {
            const response = xhr.response;
            console.log(response);
            if (!response.Success) {
                reject(genericErrorText);
                return false;
            }

            let LSFileList = window.Plugin.GetCookie('TMPFiles');
            if (LSFileList != undefined && LSFileList != null && LSFileList != '' && LSFileList != 'null')
                LSFileList += ',';
            else
                LSFileList = '';

            LSFileList += response.FileCode;
            window.Plugin.SetCookie('TMPFiles', LSFileList, 1);

            if (!response || response.error) {
                return reject(response && response.error ? response.error.message : genericErrorText);
            }
            resolve({
                default: response.ImageUrl
            });
        });

        if (xhr.upload) {
            xhr.upload.addEventListener('progress', evt => {
                if (evt.lengthComputable) {
                    loader.uploadTotal = evt.total;
                    loader.uploaded = evt.loaded;
                }
            });
        }
    }

    // Prepares the data and sends the request.
    _sendRequest(file) {
        const data = new FormData();
        data.append('file', file);
        this.xhr.send(data);
    }
}


function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}

function GWDimension() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

window.Plugin = {
    SetDocumentTitle: function (Title) {
        document.title = Title;
    },
    GetCookie: function (cname) {
        let name = cname + "=";
        let decodedCookie = decodeURIComponent(document.cookie);
        let ca = decodedCookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) == ' ') { c = c.substring(1); }
            if (c.indexOf(name) == 0) { return c.substring(name.length, c.length); }
        }
        return "";
    },
    SetCookie: function (cname, cvalue, exdays) {
        const d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        let expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
    },
    ToggleMenuClass: function (Item) {
        if ($(Item).parent().hasClass('pcoded-trigger') == true)
            $(Item).parent().removeClass('pcoded-trigger');
        else
            $(Item).parent().addClass('pcoded-trigger');
    },
    AppendScrollMenu: function () {
        new PerfectScrollbar('.navbar-content', {
            wheelSpeed: .5,
            swipeEasing: 0,
            suppressScrollX: !0,
            wheelPropagation: 1,
            minScrollbarLength: 40,
        });
    },
    //ModalHide: function (ModalID) {
    //    try {
    //        $("#" + ModalID).hide();
    //    } catch (e) {
    //        console.log(e);
    //    }
    //},
    CKEditorLunch: function (containerID, CurrentState) {
        let findContainer = createdEditors.find(i => i === containerID);
        if (findContainer != undefined && findContainer != null && findContainer === containerID) {
            return undefined;
        }

        createdEditors.push(containerID);
        $('#' + containerID + ' .toolbar-container').html("");
        currentState = CurrentState;
        try {
            if (theEditor != undefined && theEditor != null)
                theEditor.destroy();
        } catch (e) { console.log(e); }

        DecoupledDocumentEditor.create(document.querySelector('#' + containerID + ' #CKPostBody'), {
            extraPlugins: [MyCustomUploadAdapterPlugin],
            toolbar: {
                items: [
                    'heading',
                    '|',
                    'fontSize',
                    'fontFamily',
                    '|',
                    'fontColor',
                    'fontBackgroundColor',
                    '|',
                    'bold',
                    'italic',
                    'underline',
                    'strikethrough',
                    '|',
                    'alignment',
                    '|',
                    'numberedList',
                    'bulletedList',
                    '|',
                    'outdent',
                    'indent',
                    '|',
                    'todoList',
                    'link',
                    'blockQuote',
                    'imageUpload',
                    'insertTable',
                    'mediaEmbed',
                    '|',
                    'undo',
                    'redo',
                    'codeBlock',
                    'code',
                    'findAndReplace',
                    'highlight',
                    'imageInsert',
                    'subscript',
                    'superscript',
                    'specialCharacters',
                    'restrictedEditingException',
                    'sourceEditing',
                    'removeFormat',
                    'pageBreak',
                    'horizontalLine'
                ]
            },
            language: 'en',
            image: {
                toolbar: [
                    'imageStyle:inline',
                    'imageStyle:block',
                    'imageStyle:side',
                    'imageStyle:alignLeft',
                    'imageStyle:alignRight',
                    'imageStyle:alignBlockLeft',
                    'imageStyle:alignBlockRight',
                    'imageStyle:alignCenter',
                    '|',
                    'toggleImageCaption',
                    'imageTextAlternative',
                    //'linkImage',
                    'resizeImage:50',
                    'resizeImage:75',
                    'resizeImage:original',
                ],
                resizeUnit: 'px',
                resizeOptions: [
                    {
                        name: 'resizeImage:original',
                        value: null,
                        icon: 'original'
                    },
                    {
                        name: 'resizeImage:50',
                        value: '50',
                        icon: 'medium'
                    },
                    {
                        name: 'resizeImage:75',
                        value: '75',
                        icon: 'large'
                    }
                ],
            },
            table: {
                contentToolbar: [
                    'tableColumn',
                    'tableRow',
                    'mergeTableCells',
                    'tableCellProperties',
                    'tableProperties'
                ]
            },
            licenseKey: ''
        })
            .then(newEditor => {
                theEditor = newEditor;
                let toolbarContainer = document.querySelector('#' + containerID + ' .toolbar-container');
                toolbarContainer.appendChild(newEditor.ui.view.toolbar.element);
                document.querySelector('.ck-toolbar').classList.add('ck-reset_all');
            })
            .catch(error => {
                console.error(error);
            });
    },
    CKEditorGetData: function () {
        let editorBody = theEditor.getData();
        return editorBody;
    },
    CKEditorSetData: function (EditorBody) {
        theEditor.setData(EditorBody);
    },

    LastSortedList: '',
    SetDivNestable: function (ItemID, Wrapper) {
        LastSortedList = '';
        $("#" + ItemID).nestable({ group: 1 }).on('change', function (e) {
            let NewSortedList = window.JSON.stringify($(e.target).nestable('serialize'));
            if (NewSortedList !== LastSortedList) {
                LastSortedList = NewSortedList;
                Wrapper.invokeMethodAsync('SetSortedItems', NewSortedList);
            }
        });
    },
    loadRecaptcha: function (key) {
        var script = document.createElement('script');
        script.src = 'https://www.google.com/recaptcha/api.js?render=' + key;
        script.type = 'text/javascript';
        script.async = true;
        script.defer = true;
        script.charset = 'utf-8';
        document.getElementsByTagName('head')[0].appendChild(script);
    },
    generateCaptchaToken: function (key, action) {
        return grecaptcha.execute(key, { action: action });
    }
};

CKEditorInterop = (() => {
    var editors = {};

    return {
        init(id, dotNetReference) {
            window.ClassicEditor
                .create(document.getElementById(id))
                .then(editor => {
                    editors[id] = editor;
                    editor.model.document.on('change:data', () => {
                        var data = editor.getData();

                        var el = document.createElement('div');
                        el.innerHTML = data;
                        if (el.innerText.trim() === '')
                            data = null;

                        dotNetReference.invokeMethodAsync('EditorDataChanged', data);
                    });
                })
                .catch(error => console.error(error));
        },
        destroy(id) {
            editors[id].destroy()
                .then(() => delete editors[id])
                .catch(error => console.log(error));
        }
    };
})();
