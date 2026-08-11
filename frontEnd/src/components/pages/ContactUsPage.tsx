import { useState, type FormEvent } from 'react';
import type { ContactMessage } from '../../types/contact';
import { Alert } from '../ui/Alert';
import { Button } from '../ui/Button';
import { TextAreaField } from '../ui/TextAreaField';
import { TextField } from '../ui/TextField';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const EMPTY_MESSAGE: ContactMessage = { name: '', email: '', subject: '', message: '' };

export const ContactUsPage = () => {
  const [message, setMessage] = useState<ContactMessage>(EMPTY_MESSAGE);
  const [errors, setErrors] = useState<Partial<Record<keyof ContactMessage, string>>>({});
  const [isSent, setIsSent] = useState(false);

  const updateField = (field: keyof ContactMessage, value: string) => {
    setMessage((current) => ({ ...current, [field]: value }));
  };

  const validate = (): boolean => {
    const nextErrors: Partial<Record<keyof ContactMessage, string>> = {};

    if (!message.name.trim()) {
      nextErrors.name = 'Please tell us your name.';
    }

    if (!message.email.trim()) {
      nextErrors.email = 'Please give us an email address to reply to.';
    } else if (!EMAIL_PATTERN.test(message.email)) {
      nextErrors.email = 'That email address does not look valid.';
    }

    if (!message.subject.trim()) {
      nextErrors.subject = 'Please add a subject.';
    }

    if (message.message.trim().length < 10) {
      nextErrors.message = 'Please write at least 10 characters so we can help properly.';
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validate()) {
      return;
    }

    // There is no contact endpoint on the API yet, so this confirms locally
    // rather than pretending to have delivered anything.
    setIsSent(true);
    setMessage(EMPTY_MESSAGE);
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-16 sm:px-6">
      <h1 className="text-3xl font-semibold tracking-tight text-ink sm:text-4xl">Contact us</h1>

      <p className="mt-4 text-ink-soft">
        Questions about monitoring, pricing, or getting a job wired up? Send us a note and we will get
        back to you.
      </p>

      {isSent ? (
        <div className="mt-8">
          <Alert tone="success">
            Thanks — your message has been recorded. Sending is not connected to a backend endpoint
            yet, so nothing has left your browser.
          </Alert>
        </div>
      ) : null}

      <form onSubmit={handleSubmit} noValidate className="mt-8 flex flex-col gap-5">
        <TextField
          label="Your name"
          name="name"
          value={message.name}
          error={errors.name}
          autoComplete="name"
          onChange={(event) => updateField('name', event.target.value)}
        />

        <TextField
          label="Email address"
          name="email"
          type="email"
          value={message.email}
          error={errors.email}
          autoComplete="email"
          onChange={(event) => updateField('email', event.target.value)}
        />

        <TextField
          label="Subject"
          name="subject"
          value={message.subject}
          error={errors.subject}
          onChange={(event) => updateField('subject', event.target.value)}
        />

        <TextAreaField
          label="Message"
          name="message"
          value={message.message}
          error={errors.message}
          onChange={(value) => updateField('message', value)}
        />

        <div>
          <Button type="submit">Send message</Button>
        </div>
      </form>
    </div>
  );
};
