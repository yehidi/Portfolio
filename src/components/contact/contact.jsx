import React, {useRef} from 'react'
import './Contact.css'
import {MdOutlineEmail} from 'react-icons/md'
import {RiMessengerLine} from 'react-icons/ri'
import { BsWhatsapp } from 'react-icons/bs'
import emailjs from '@emailjs/browser'



const Contact = () => {

  const form = useRef();
  
  const sendEmail = (e) => {
    e.preventDefault();

  emailjs.sendForm('service_taz0v8p', 'template_3kun8tw', form.current, '33Zm4eP3tl5mIDzaz')
  e.target.reset();
  
  };
  
  return (
    <section id='contact'>
      <h5>Get In Touch</h5>
      <h2>Contact Me</h2>

      <div className='container contact__container'>
        <div className='contact__options'>
          <article className='contact__option'>
            <MdOutlineEmail className='contact__option-icon'/>
            <h4>Email</h4>
            <h5>yehidi@gmail.com</h5>
            <a href='mailto:yehidi@gmail.com' target='_blank' rel='noreferrer'>Send a message</a>
          </article>
          <article className='contact__option'>
            <RiMessengerLine className='contact__option-icon'/>
            <h4>Messenger</h4>
            <h5>yehidi@gmail.com</h5>
            <a href='https://m.me/OmriYehidi' target='_blank' rel='noreferrer'>Send a message</a>
          </article>
          <article className='contact__option'>
            <BsWhatsapp className='contact__option-icon'/>
            <h4>WhatsApp</h4>
            <h5>+972-52-5855981</h5>
            <a href='https://api.whatsapp.com/send?phone=+972525855981' target='_blank'>Send a message</a>
          </article>
        </div>

        <form ref={form} onSubmit={sendEmail}>
          <input type="text" name='name' placeholder='Full Name' required />
          <input type="email" name='email' placeholder='Email' required />
          <textarea name='message' rows='7' placeholder='Type a Message' required />
          <button type='submit' className='btn btn-primary-contact'>Send Message</button>

        </form>
      </div>
    </section>
  )
}

export default Contact