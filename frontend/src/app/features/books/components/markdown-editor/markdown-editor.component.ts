import { AfterViewInit, Component, ElementRef, input, OnDestroy, output, ViewChild } from '@angular/core';
import EasyMDE from 'easymde';

@Component({
  selector: 'app-markdown-editor',
  imports: [],
  templateUrl: './markdown-editor.component.html',
})
export class MarkdownEditorComponent implements AfterViewInit, OnDestroy {
  @ViewChild('editor') textarea!: ElementRef<HTMLTextAreaElement>;

  content = input<string>('');
  contentChanged = output<string>();
  editorReady = output<void>();

  private easyMDE!: EasyMDE;

  ngAfterViewInit() {
    this.initializeEditor();
  }

  ngOnDestroy() {
    if (this.easyMDE) {
      this.easyMDE.cleanup();
    }
  }

  private initializeEditor() {
    this.easyMDE = new EasyMDE({
      element: this.textarea.nativeElement,
      initialValue: this.content(),
      spellChecker: false,


      // toolbar: [
      //   'bold', 'italic', 'heading', '|',
      //   'code', 'quote', 'unordered-list', 'ordered-list', '|',
      //   'link', '|',
      //   'preview', 'side-by-side', 'fullscreen', '|',
      //   'guide'
      // ],

      // Toolbar SIN opción de imágenes
      toolbar: [
        'bold', 'italic', '|',
        'quote', 'unordered-list', 'ordered-list', '|',
        {
          name: 'indent',
          action: (editor) => {
            const cm = editor.codemirror;
            const cursor = cm.getCursor();

            // Insertar 4 espacios no rompibles
            const indent = '&nbsp;&nbsp;&nbsp;&nbsp;';

            cm.replaceRange(indent, cursor);
          },
          className: 'fa fa-indent',
          title: 'Añadir sangría'
        },
        'preview', 'side-by-side', '|',
        'guide'
      ],

      // Deshabilitar todas las funcionalidades de imagen
      uploadImage: false,
      imageUploadFunction: undefined,
      imagePathAbsolute: false,
      // imageDrop: false,

      // placeholder: 'Escribe tu contenido en markdown...',
      status: false,
      minHeight: '400px',
      sideBySideFullscreen: false,
      toolbarTips: true,
      renderingConfig: {
        singleLineBreaks: false,
        codeSyntaxHighlighting: false
      }
    });

    this.easyMDE.codemirror.on('change', () => {
      this.contentChanged.emit(this.easyMDE.value());
    });

    setTimeout(() => {
      this.editorReady.emit();
    });
  }

  getValue(): string {
    return this.easyMDE?.value() || '';
  }

  setValue(content: string): void {
    if (this.easyMDE) {
      this.easyMDE.value(content);
    }
  }

  clear(): void {
    if (this.easyMDE) {
      this.easyMDE.value('');
    }
  }
}
