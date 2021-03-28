import { Component, HostListener, Injector, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ValidatorFn } from '@angular/forms';

import { BotColour } from '../../controllers.service';
import { ModalComponent } from '../../modal/modal.component';

export interface IButton {
    text: string;
    role?: 'cancel' | 'submit';
    colour?: BotColour;
    className?: string;
}

export interface IInput {
    name: string;
    type: 'text' | 'number' | 'email' | 'password';
    autocomplete?: string;
    placeholder?: string;
    value?: string;
    validators?: ValidatorFn[];
    className?: string;
    label?: string;
    text?: string;
    errorMap?: { [key: string]: string };
}

@Component({
    selector: 'bot-alert-component',
    templateUrl: 'alert.component.html',
    styleUrls: ['alert.component.scss'],
})
export class AlertComponent extends ModalComponent<any> implements OnInit {
    @Input() title: string;
    @Input() message: string;
    @Input() buttons: IButton[] = [];
    @Input() inputs: IInput[] = [];
    @Input() focusFirst: boolean = false;

    form: FormGroup;

    constructor(injector: Injector, private fb: FormBuilder) {
        super(injector);
    }

    ngOnInit() {
        super.ngOnInit();

        this.inputs = (this.inputs || []).filter((i) => !!i);
        this.form = this.fb.group(
            this.inputs.reduce((acc, curr) => {
                acc[curr.name] = [curr.value, curr.validators || []];
                return acc;
            }, {})
        );

        setTimeout(() => {
            const parent = document.getElementById(this.id);
            const hasInputs = this.inputs.length > 0;
            const el = this.findTag(parent, hasInputs ? 'INPUT' : 'BUTTON', this.focusFirst || hasInputs);

            if (el) el.focus();
        }, 0);
    }

    @HostListener('document:keydown.escape')
    cancel() {
        this.result.next(null);
    }

    submit() {
        this.result.next(this.form.value);
    }

    errors(field: string): string[] {
        const errors = this.form.get(field).errors || {};
        if (errors['required']) return ['Required'];

        return Object.keys(errors).reduce((acc, curr) => {
            const inp = this.inputs.find((i) => i.name === field);
            if (inp && inp.errorMap && inp.errorMap[curr]) acc.push(inp.errorMap[curr]);
            else {
                switch (curr) {
                    case 'minlength':
                        acc.push('Too short');
                        break;

                    case 'maxlength':
                        acc.push('Too long');
                        break;
                }
            }

            return acc;
        }, []);
    }

    private findTag(startTag: Element, tagName: string, breakOnFirst: boolean): any {
        const nodes = startTag.children;

        let tag = null;
        for (let i = 0; i < nodes.length; i++) {
            if (nodes[i].tagName === tagName) {
                tag = nodes[i];
                if (breakOnFirst) break;
            }

            const t = this.findTag(nodes[i], tagName, breakOnFirst);
            if (t) {
                tag = t;
                if (breakOnFirst) break;
            }
        }

        return tag;
    }
}
