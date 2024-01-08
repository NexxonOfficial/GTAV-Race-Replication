class PropHoverItem {
    constructor(isKeyboard) {
        this.Keyboard = isKeyboard
        this.Init()
    }

    Init() {
        let overall = document.createElement('div')
        overall.classList.add('prop-hover-item-overall')

        document.body.appendChild(overall)
        this.Overall = overall
    }

    CreateMain(top, mid, bottom, topColor = 'white') {
        let main = document.createElement('div')
        main.classList.add('prop-hover-item-main')

        let topText = document.createElement('h3')
        topText.classList.add('prop-hover-item-top-text')
        topText.innerHTML = top
        topText.style.color = topColor

        let midText = document.createElement('h2')
        midText.classList.add('prop-hover-item-badge-mid-text')
        midText.innerHTML = mid

        let bottomText = document.createElement('h4')
        bottomText.classList.add('prop-hover-item-badge-bottom-text')

        if(bottom.includes('~')) {
            let split = bottom.split(' ')
            for(let i=0; i < split.length; i++) {
                if(split[i].includes('~')) {
                    let replaced = split[i].replaceAll('~', '')
                    if(!this.Keyboard) {
                        switch(replaced) {
                            case 'E':
                                replaced = 'DPAD RIGHT'
                                break
                            default: break;
                        }
                    }
                    split[i] = `<span>${replaced}</span>`
                }
            }

            let useable = split.join(' ')

            bottomText.innerHTML = useable
        } else {
            bottomText.innerHTML = bottom
        }

        main.appendChild(topText)
        main.appendChild(midText)
        main.appendChild(bottomText)

        this.Overall.appendChild(main)
        this.Main = main
    }

    Dispose() {
        this.Overall.style.opacity = 0
        setTimeout(() => {
            document.body.removeChild(this.Overall)
        }, 201);
    }
}