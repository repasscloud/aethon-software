window.tinymceInterop = {
    init: function (elementId, dotNetRef) {
        if (typeof tinymce === 'undefined') return;
        tinymce.init({
            selector: '#' + elementId,
            base_url: '/lib/tinymce',
            suffix: '.min',
            plugins: 'lists link autolink image table codesample emoticons',
            toolbar: 'undo redo | styles | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist | outdent indent | link | table | removeformat',
            menubar: false,
            height: 350,
            branding: false,
            promotion: false,
            skin: 'oxide',
            content_css: 'default',
            setup: function (editor) {
                editor.on('change input', function () {
                    dotNetRef.invokeMethodAsync('OnEditorChange', editor.getContent());
                });
            }
        });
    },
    destroy: function (elementId) {
        if (typeof tinymce === 'undefined') return;
        const editor = tinymce.get(elementId);
        if (editor) editor.remove();
    },
    setContent: function (elementId, content) {
        if (typeof tinymce === 'undefined') return;
        const editor = tinymce.get(elementId);
        if (editor) editor.setContent(content || '');
    },
    getContent: function (elementId) {
        if (typeof tinymce === 'undefined') return '';
        const editor = tinymce.get(elementId);
        return editor ? editor.getContent() : '';
    }
};
